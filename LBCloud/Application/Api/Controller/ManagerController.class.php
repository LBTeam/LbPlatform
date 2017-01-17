<?php
/**
 * 编辑端接口控制器
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Api\Controller;
use Api\Service\AliyunOSS;

class ManagerController extends CommonController
{
	private $param;
	private $user_id;
	private $user_model;
	private $media_bucket;
	private $program_bucket;
	public function _initialize(){
		$request = file_get_contents('php://input');
		/*登录令牌（token）检测开始*/
		$token = I("request.token");
		$this->user_model = D("User");
		if(in_array(ACTION_NAME, C("manager_not_logged"))){
			//未登录可访问模块
			//不检查登录令牌
			
		}else{
			//检查token
			if($token){
				$this->user_id = $this->user_model->check_token($token);
				if($this->user_id === false){
					$respones = array('err_code'=>'010002', 'msg'=>"Token error");
					$this->ajaxReturn($respones);exit;
				}else{
					if(!$this->user_id){
						$respones = array('err_code'=>'010003', 'msg'=>"Token timeout");
						$this->ajaxReturn($respones);exit;
					}
				}
			}else{
				$respones = array('err_code'=>'010002', 'msg'=>"Token error");
				$this->ajaxReturn($respones);exit;
			}
		}
		//$this->user_id = 28;
		/*登录令牌（token）检测结束*/
		$this->param = json_decode($request, true);
		/*请求数据检测开始*/
		if(in_array(ACTION_NAME, C("manager_param_empty"))){
			//空请求数据可访问模块
			
		}else{
			//检查请求数据
			if(empty($this->param) === true){
				$respones = array('err_code'=>'010001', 'msg'=>"Protocol content error");
				$this->ajaxReturn($respones);exit;
			}
		}
		//file_put_contents('./1.log', json_encode($this->param)."\r\n", FILE_APPEND);
		/*请求数据检测结束*/
		$this->media_bucket = C("oss_media_bucket");
		$this->program_bucket = C("oss_program_bucket");
	}
	
	/**
	 * oss配置
	 */
	public function configuration(){
		$configure = array();
		$configure['accessKeyId']		= C("aliyun_oss_id");
		$configure['accessKeySecret']	= C("aliyun_oss_secret");
		$configure['endpoint']			= C("aliyun_oss_endpoint");
		$configure['bucket']			= C("aliyun_oss_bucket");
		$configure['mediaBucket']		= C("oss_media_bucket");
		$configure['programBucket']		= C("oss_program_bucket");
		//$configure = encrypt(json_encode($configure));
		$respones = array("err_code"=>"000000", "msg"=>"ok", 'data'=>$configure);
		$this->ajaxReturn($respones);
	}

	/**
	 * 登录
	 */
	public function login(){
		$obj = $this->param;
		$username = $obj['user'];
		$password = $obj['pwd'];
		$respones = array();
		$user_info = $this->user_model->user_by_email($username);
		if($user_info){
			$db_pwd = $user_info['password'];
			if(sp_compare_password($password, $db_pwd)){
				$access_token = create_access_token($username);
				$token = $access_token['token'];
				$expire = $access_token['expire'];
				
				$data = array();
				$data['uid'] = $user_info['uid'];
				$data['lasttime'] = NOW_TIME;
				$data['lastip'] = get_client_ip();
				$data['token'] = $token;
				$data['expire'] = $expire;
				$res = $this->user_model->save($data);
				if($res){
					$return = array('token'=>$token, 'expire'=>7200);
					$respones = array('err_code'=>'000000', 'msg'=>"ok", 'data'=>$return);
				}else{
					$respones = array('err_code'=>'010103', 'msg'=>"Login failed");
				}
			}else{
				$respones = array('err_code'=>'010102', 'msg'=>"Password error");
			}
		}else{
			$respones = array('err_code'=>'010101', 'msg'=>"User does not exist");
		}
		$this->ajaxReturn($respones);
	}
	
	/**
	 * 刷新token
	 */
	public function refresh_token(){
		$token = I("request.token", "");
		if($token){
			$user_id = $this->user_model->check_token($token);
			if($user_id === false){
				$respones = array('err_code'=>'010002', 'msg'=>"Token not found");
			}else{
				if($user_id){
					//token未过期
					$respones = array('err_code'=>'010005', 'msg'=>"Token not expired");
				}else{
					$user_info = $this->user_model->user_by_token($token, "uid,email,phone");
					$username = $user_info['email'] ? trim($user_info['email']) : trim($user_info['phone']);
					$access_token = create_access_token($username);
					$token = $access_token['token'];
					$expire = $access_token['expire'];
					
					$data = array();
					$data['uid'] = $user_info['uid'];
					$data['token'] = $token;
					$data['expire'] = $expire;
					$res = $this->user_model->save($data);
					if($res){
						$return = array('token'=>$token, 'expire'=>7200);
						$respones = array('err_code'=>'000000', 'msg'=>"ok", 'data'=>$return);
					}else{
						$respones = array('err_code'=>'010006', 'msg'=>"Token refresh failed");
					}
				}
			}
		}else{
			$respones = array('err_code'=>'010004', 'msg'=>"Token empty");
		}
		$this->ajaxReturn($respones);
	}

	/**
	 * 心跳
	 */
	public function heartbeat(){
		$respones = array('err_code'=>'000000', 'msg'=>"ok");
		$this->ajaxReturn($respones);
	}
	
	/**
	 * 上传
	 */
	public function upload(){
		$result = array();
		$obj = $this->param;
		$media_model = D("Media");
		$program_model = D("Program");
		$AliyunOSS = new AliyunOSS();
		//file_put_contents('./1.log', json_encode($obj)."\r\n", FILE_APPEND);
		$plan = $obj['scheduleFile'];
		$medias = $obj['mediaFileList'];
		$user_id = $this->user_id;
		//处理播放方案
		$planpath = $plan['FilePath'];
		$planname = end(explode('/', str_replace('\\', '/', $planpath)));
		//file_put_contents('./1.log', $planname."\r\n", FILE_APPEND);
		$plansize = $plan['FileSize'];
		$planmd5 = strtolower($plan['FileMD5']);
		$plansubfix = end(explode('.', $planname));
		$plantype = $plan['PlanType'] ? $plan['PlanType'] : 0;
		$planmedias = $plan['MediaList'];
		foreach($planmedias as &$v){
			$v['MediaName'] = end(explode('/', str_replace('\\', '/', $v['MediaName'])));
		}
		$plan_data = $this->_upload_program($program_model, $AliyunOSS, $user_id, $planname, $plansize, $planmd5, $plansubfix, $planmedias, $planpath, $plantype);
		if($plan_data){
			$plan_data['type'] = intval($plan['Type']);
			$result[] = $plan_data;
		}
		//处理媒体
		foreach ($medias as $val) {
			$mediapath = $val['FilePath'];
			$medianame = end(explode('/', str_replace('\\', '/', $mediapath)));
			$mediasize = $val['FileSize'];
			$mediamd5 = strtolower($val['FileMD5']);
			$mediasubfix = end(explode('.', $medianame));
			$media_data = $this->_upload_media($media_model, $AliyunOSS, $user_id, $medianame, $mediasize, $mediamd5, $mediasubfix, $mediapath);
			if($media_data){
				$media_data['type'] = intval($val['Type']);
				$result[] = $media_data;
			}
		}
		/*foreach ($obj as $val) {
			$user_id = $this->user_id;
			$filepath = $val['FilePath'];
			$filename = end(explode('/', str_replace('\\', '/', $filepath)));
			$filesize = $val['FileSize'];
			$filemd5 = strtolower($val['FileMD5']);
			$filesubfix = end(explode('.', $filename));
			$filetype = $val['Type'];   //0-播放方案,1-图片,2-视频,3-文字
			if($filetype == 0){
				//file_put_contents('./1.log', json_encode($val)."\r\n", FILE_APPEND);
				//播放方案
				$medias = $val['MediaList'];
				$plantype = $val['PlanType'] ? $val['PlanType'] : 0;
				foreach($medias as &$v){
					$v['MediaName'] = end(explode('/', str_replace('\\', '/', $v['MediaName'])));
				}
				$program_data = $this->_upload_program($program_model, $AliyunOSS, $user_id, $filename, $filesize, $filemd5, $filesubfix, $medias, $filepath, $plantype);
				if($program_data){
					$program_data['type'] = intval($filetype);
					$result[] = $program_data;
				}
			}else{
				//媒体
				$media_data = $this->_upload_media($media_model, $AliyunOSS, $user_id, $filename, $filesize, $filemd5, $filesubfix, $filepath);
				//file_put_contents('./1.log', json_encode($media_data)."\r\n", FILE_APPEND);
				if($media_data){
					$media_data['type'] = intval($filetype);
					$result[] = $media_data;
				}
			}
		}*/
		//file_put_contents('./1.log', json_encode($result)."\r\n", FILE_APPEND);
		$this->ajaxReturn($result);
	}
	
	/**
	 * 完成上传
	 */
	public function complete_upload(){
		$obj = $this->param;
		//file_put_contents('./1.log', json_encode($obj)."\r\n", FILE_APPEND);
		//$filename = $obj['FileName'];
		$filepath = $obj['FileName'];
		$filename = end(explode('/', str_replace('\\', '/', $filepath)));
		$filemd5 = strtolower($obj['FileMD5']);
		$filetype = $obj['FileType'];
		$fileparts = $obj['Parts'];
		$user_id = $this->user_id;
		if($filetype == 0){
			//播放方案
			$screens = $obj['Screens'];
			$prog_model = D("Program");
			$prog_info = $prog_model->program_by_name_md5($filename, $filemd5, $user_id);
			if($prog_info['status'] == 0){
				$prog_map = array('id'=>$prog_info['id']);
				$prog_res = $prog_model->where($prog_map)->setField('status', 1);
				if($prog_info['size'] >= C("oss_100K_size")){
					$AliyunOSS = new AliyunOSS();
					//合并文件
					$object = $prog_info['object'];
					$uploadId = $prog_info['upload_id'];
					$parts = array();
					foreach($fileparts as $val){
						$parts[] = array('PartNumber'=>$val['partNumber'],'ETag'=>strtoupper($val['MD5']));
					}
					$oss_res = $AliyunOSS->complete_upload($object, $uploadId, $parts, $this->program_bucket);
				}
			}
			//方案类型，0-普通方案，7-紧急插播，8-离线方案
			$plantype = $obj['PlanType'] ? $obj['PlanType'] : 0; 
			//播放方案下发
			$cmds = array();
			foreach($screens as $val){
				$cmds[] = array(
					'screen_id'	=> $val,
					'type'		=> $plantype,
					'param'		=> json_encode(array('program_id'=>$prog_info['id'])),
					'publish'	=> NOW_TIME,
					'execute'	=> NOW_TIME,
					'expired'	=> NOW_TIME,
					'status'	=> 0
				);
			}
			if($cmds){
				$cmd_model = D("Command");
				$cmd_del = $cmd_model->remove_cmd($screens, $plantype, 0);
				$cmd_add = $cmd_model->release_cmd($cmds);
			}
			$respones = array("err_code"=>"000000","msg"=>"success");
		}else{
			//媒体
			$media_model = D("Media");
			$media_info = $media_model->media_by_name_md5($filename, $filemd5, $user_id);
			//file_put_contents('./1.log', json_encode($media_info)."\r\n", FILE_APPEND);
			$oss_res = true;
			if($media_info['size'] > C("oss_100K_size")){
				$AliyunOSS = new AliyunOSS();
				//合并文件
				$object = $media_info['object'];
				$uploadId = $media_info['upload_id'];
				$parts = array();
				foreach($fileparts as $val){
					$parts[] = array('PartNumber'=>$val['PartNumber'],'ETag'=>strtoupper($val['MD5']));
				}
				//file_put_contents('./1.log', json_encode($parts)."\r\n", FILE_APPEND);
				$oss_res = $AliyunOSS->complete_upload($object, $uploadId, $parts, $this->media_bucket);
				//file_put_contents('./1.log', json_encode($oss_res)."\r\n", FILE_APPEND);
			}
			if($oss_res){
				$media_map = array('id'=>$media_info['id']);
				$media_res = $media_model->where($media_map)->setField('status', 1);
				$respones = array("err_code"=>"000000","msg"=>"success");
			}else{
				$respones = array("err_code"=>"010201","msg"=>"Merge split file failed");
			}
		}
		$this->ajaxReturn($respones);
	}

	/**
	 * 终端列表
	 */
	public function screens(){
		$user_id = $this->user_id;
		$screen_model = D("Screen");
		$result = $screen_model->user_all_screen($user_id);
		$regions = D("Region")->all_region_name();
		$screens = array();
		//testing start
		foreach($result as $val){
			$province = $regions[$val['province']];
			$city = $regions[$val['city']];
			$district = $regions[$val['district']];
			$screens[] = array(
				'id'		=> $val['id'],
				'name'		=> $val['name'],
				'size_x'	=> intval($val['size_x']),
				'size_y'	=> intval($val['size_y']),
				'resolu_x'	=> intval($val['resolu_x']),
				'resolu_y'	=> intval($val['resolu_y']),
				'type'		=> intval($val['type']),
				'operate'	=> intval($val['operate']),
				'longitude'	=> floatval($val['longitude']),
				'latitude'	=> floatval($val['latitude']),
				'address'	=> "{$province}{$city}{$district}{$val['address']}",
			);
		}
		$this->ajaxReturn($screens);
		//testing end
		$groups = array();
		foreach($result as $val){
			$province = $regions[$val['province']];
			$city = $regions[$val['city']];
			$district = $regions[$val['district']];
			if(is_null($val['group_id'])){
				$screens[] = array(
					'id'		=> $val['id'],
					'name'		=> $val['name'],
					'size_x'	=> intval($val['size_x']),
					'size_y'	=> intval($val['size_y']),
					'resolu_x'	=> intval($val['resolu_x']),
					'resolu_y'	=> intval($val['resolu_y']),
					'type'		=> intval($val['type']),
					'operate'	=> intval($val['operate']),
					'longitude'	=> floatval($val['longitude']),
					'latitude'	=> floatval($val['latitude']),
					'address'	=> "{$province}{$city}{$district}{$val['address']}",
				);
			}else{
				if(!$groups[$val['group_id']]){
					$groups[$val['group_id']] = array(
						'id'		=> $val['group_id'],
						'name'		=> $val['group_name']
					);
				}
				$groups[$val['group_id']]['screens'][] = array(
					'id'		=> $val['id'],
					'name'		=> $val['name'],
					'size_x'	=> intval($val['size_x']),
					'size_y'	=> intval($val['size_y']),
					'resolu_x'	=> intval($val['resolu_x']),
					'resolu_y'	=> intval($val['resolu_y']),
					'type'		=> intval($val['type']),
					'operate'	=> intval($val['operate']),
					'longitude'	=> floatval($val['longitude']),
					'latitude'	=> floatval($val['latitude']),
					'address'	=> "{$province}{$city}{$district}{$val['address']}",
				);
			}
		}
		$respones = array();
		$respones['groups'] = array_values($groups);
		$respones['screens'] = $screens;
		$this->ajaxReturn($respones);
	}

	/**
	 * 播放方案备份
	 */
	public function backup(){
		$obj = $this->param;
		$user_id = $this->user_id;
		$respones = array();
		$path = $obj['FileName'];
		$name = end(explode('/', str_replace('\\', '/', $path)));
		$md5 = strtolower($obj['FileMD5']);
		$model = D("Program");
		$info = $model->program_by_name_md5($name, $md5, $user_id);
		if($info){
			$release = $model->program_can_release($info['id'], $user_id);
			if($release){
				$is_backup = $model->plan_is_backup($name, $user_id);
				if($is_backup){
					if($info['id'] == $is_backup){
						$respones = array("err_code"=>"000000","msg"=>"ok");
					}else{
						$data = array();
						$ddata = array();
						$data['id'] = $info['id'];
						$data['backup'] = 1;
						$ddata['id'] = $is_backup;
						$ddata['backup'] = 0;
						$m = new \Think\Model();
						$m->startTrans();
						$res = $model->save($data);
						$rres = $model->save($ddata);
						if($res && $rres){
							$m->commit();
							$respones = array("err_code"=>"000000","msg"=>"ok");
						}else{
							$m->rollback();
							$respones = array("err_code"=>"010303","msg"=>"Program backup failed");
						}
					}
				}else{
					$data = array();
					$data['id'] = $info['id'];
					$data['backup'] = 1;
					$res = $model->save($data);
					if($res){
						$respones = array("err_code"=>"000000","msg"=>"ok");
					}else{
						$respones = array("err_code"=>"010303","msg"=>"Program backup failed");
					}
				}
			}else{
				$respones = array("err_code"=>"010302","msg"=>"Program not complete and cannot be backup");
			}
		}else{
			$respones = array("err_code"=>"010301","msg"=>"Program not found");
		}
		$this->ajaxReturn($respones);
	}

	/**
	 * 播放方案列表
	 */
	public function plans(){
		$user_id = $this->user_id;
		$program_model = D("Program");
		$media_model = D("Media");
		$AliyunOSS = new AliyunOSS();
		$all_program = $program_model->all_backup_plan($user_id);
		$programs  = array();
		foreach($all_program as $val){
			$release = $program_model->program_can_release($val['id'], $user_id);
			if($release){
				$temp = array();
				$temp['ProgramId']		= $val['id'];
				$temp['ProgramName']	= stripslashes($val['name']);
				$temp['ProgramUrl']		= $AliyunOSS->download_uri($this->program_bucket, $val['object']);
				$medias = json_decode($val['info'], true);
				$media_list = array();
				foreach($medias as $v){
					$media_info = $media_model->media_by_name_md5($v['MediaName'], $v['MediaMD5'], $user_id);
					$media_list[] = array(
						'MediaId'	=> $media_info['id'],
						'MediaName'	=> stripslashes($media_info['name']),
						'MediaUrl'	=> $AliyunOSS->download_uri($this->media_bucket, $media_info['object'])
					);
				}
				$temp['MediaList'] = $media_list;
				$programs[] = $temp;
			}
		}
		$this->ajaxReturn($programs);
	}
	
	/**
	 * 上传播放方案
	 */
	private function _upload_program($model, $oss_obj, $user_id, $filename, $filesize, $filemd5, $subfix, $medias, $filepath, $plantype=0){
		$result = array();
		$program_id = $model->program_exists($filename, $filemd5, $user_id);
		if($program_id){
			//文件存在
			$program_info = $model->program_detail($program_id);
			if($program_info['status'] == 0){
				$object = $program_info['object'];
				$uploadId = $program_info['upload_id'];
				if($filesize <= C("oss_100K_size")){
					//不使用分片
					$sign_url = $oss_obj->upload_sign_uri($object, $this->program_bucket);
					$parts = array();
					$parts[] = array(
						'partNumber'	=> 1,
						'seekTo'		=> 0,
						'length'		=> intval($filesize),
						'url'			=> $sign_url
					);
					$result = array(
						//'name'	=> $filename,
						//'path'	=> $filepath,
						'name'	=> $filepath,
						'md5'	=> $filemd5,
						'key'	=> $object,
						'parts'	=> $parts
					);
				}else{
					//未上传完成
					//获取oss端上传成功的分片
					//对比得出还需要上传的分片及上传地址等
					$part_size = $oss_obj->part_size($filesize);
					$upload_parts = $oss_obj->generate_upload_part($filesize, $part_size);
					$part_lists = $oss_obj->part_list($object, $uploadId, $this->program_bucket);
					$part_list = array();
					if($part_lists){
						foreach($part_lists as $val){
							$part_list[] = $val['partNumber'];
						}
					}
					$parts = array();
					foreach($upload_parts as $key=>$val){
						$part_number = $key+1;
						if(!in_array($part_number, $part_list)){
							//需要上传的分片
							$sign_url = $oss_obj->upload_part_sign($object, $uploadId, $part_number, $this->program_bucket);
							$parts[] = array(
								'partNumber'	=> $part_number,
								'seekTo'		=> $val['seekTo'],
								'length'		=> $val['length'],
								'url'			=> $sign_url
							);
						}
					}
					if($parts){
						$result = array(
							//'name'	=> $filename,
							//'path'	=> $filepath,
							'name'	=> $filepath,
							'md5'	=> $filemd5,
							'key'	=> $object,
							'parts'	=> $parts
						);
					}
				}
			}else{
				//已上传完成
				//不做处理
			}
			$update = array();
			$update['id'] = $program_id;
			$update['info'] = json_encode($medias);
			$model->save($update);
		}else{
			//文件不存在
			if($filesize <= C("oss_100K_size")){
				//不使用分片
				$object = oss_object($subfix);
				$program_data = array();
				$program_data['user_id'] = $user_id;
				$program_data['name'] = mysql_escape_string($filename);
				//$program_data['name'] = $filename;
				$program_data['object'] = $object;
				$program_data['upload_id'] = '';
				$program_data['info'] = json_encode($medias);
				$program_data['md5'] = $filemd5;
				$program_data['type'] = $plantype;
				$program_data['size'] = $filesize;
				$program_data['publish'] = NOW_TIME;
				$program_id = $model->add($program_data);
				//file_put_contents('./1.log', $model->getLastSql()."\r\n", FILE_APPEND);
				$sign_url = $oss_obj->upload_sign_uri($object, $this->program_bucket);
				$parts = array();
				$parts[] = array(
					'partNumber'	=> 1,
					'seekTo'		=> 0,
					'length'		=> intval($filesize),
					'url'			=> $sign_url
				);
				$result = array(
					//'name'	=> $filename,
					//'path'	=> $filepath,
					'name'	=> $filepath,
					'md5'	=> $filemd5,
					'key'	=> $object,
					'parts'	=> $parts
				);
			}else{
				//文件分片
				$part_size = $oss_obj->part_size($filesize);
				$upload_parts = $oss_obj->generate_upload_part($filesize, $part_size);
				//获取uploadId
				$upload_info = $oss_obj->get_upload_id($subfix, $this->program_bucket);
				//上传文件信息入库
				$object = $upload_info['Key'];
				$uploadId = $upload_info['UploadId'];
				$program_data = array();
				$program_data['user_id'] = $user_id;
				$program_data['name'] = mysql_escape_string($filename);
				//$program_data['name'] = $filename;
				//file_put_contents('./1.log', $model->getLastSql()."\r\n", FILE_APPEND);
				$program_data['object'] = $object;
				$program_data['upload_id'] = $uploadId;
				$program_data['info'] = json_encode($medias);
				$program_data['md5'] = $filemd5;
				$program_data['type'] = $plantype;
				$program_data['size'] = $filesize;
				$program_data['publish'] = NOW_TIME;
				$program_id = $model->add($program_data);
				//获取每片文件签名地址
				$parts = array();
				foreach($upload_parts as $key=>$val){
					$part_number = $key+1;
					$sign_url = $oss_obj->upload_part_sign($object, $uploadId, $part_number, $this->program_bucket);
					$parts[] = array(
						'partNumber'	=> $part_number,
						'seekTo'		=> $val['seekTo'],
						'length'		=> $val['length'],
						'url'			=> $sign_url
					);
				}
				if($parts){
					$result = array(
						//'name'	=> $filename,
						//'path'	=> $filepath,
						'name'	=> $filepath,
						'md5'	=> $filemd5,
						'key'	=> $object,
						'parts'	=> $parts
					);
				}
			}
		}
		return $result;
	}

	/**
	 * 上传媒体
	 */
	private function _upload_media($model, $oss_obj, $user_id, $filename, $filesize, $filemd5, $subfix, $filepath){
		$result = array();
		$media_id = $model->media_exists($filename, $filemd5, $user_id);
		//file_put_contents('./1.log', json_encode($media_id)."\r\n", FILE_APPEND);
		if($media_id){
			$media_info = $model->media_detail($media_id);
			//file_put_contents('./1.log', json_encode($media_info)."\r\n", FILE_APPEND);
			$object = $media_info['object'];
			$uploadId = $media_info['upload_id'];
			if($media_info['status'] == 0){
				if($filesize <= C("oss_100K_size")){
					//不使用分片
					$sign_url = $oss_obj->upload_sign_uri($object, $this->media_bucket);
					$parts = array();
					$parts[] = array(
						'partNumber'	=> 1,
						'seekTo'		=> 0,
						'length'		=> intval($filesize),
						'url'			=> $sign_url
					);
					$result = array(
						//'name'	=> $filename,
						//'path'	=> $filepath,
						'name'	=> $filepath,
						'md5'	=> $filemd5,
						'key'	=> $object,
						'parts'	=> $parts
					);
				}else{
					$part_size = $oss_obj->part_size($filesize);
					$upload_parts = $oss_obj->generate_upload_part($filesize, $part_size);
					//file_put_contents('./1.log', json_encode($upload_parts)."\r\n", FILE_APPEND);
					$part_lists = $oss_obj->part_list($object, $uploadId, $this->media_bucket);
					//file_put_contents('./1.log', json_encode($part_lists)."\r\n", FILE_APPEND);
					$part_list = array();
					foreach($part_lists as $val){
						$part_list[] = $val['partNumber'];
					}
					$parts = array();
					foreach($upload_parts as $key=>$val){
						$part_number = $key+1;
						if(!in_array($part_number, $part_list)){
							//需要上传的分片
							$sign_url = $oss_obj->upload_part_sign($object, $uploadId, $part_number, $this->media_bucket);
							$parts[] = array(
								'partNumber'	=> $part_number,
								'seekTo'		=> $val['seekTo'],
								'length'		=> $val['length'],
								'url'			=> $sign_url
							);
						}
					}
					//file_put_contents('./1.log', json_encode($parts)."\r\n", FILE_APPEND);
					if($parts){
						$result = array(
							//'name'	=> $filename,
							//'path'	=> $filepath,
							'name'	=> $filepath,
							'md5'	=> $filemd5,
							'key'	=> $object,
							'parts'	=> $parts
						);
					}
				}
			}else{
				//已上传完成
				$object_info = $oss_obj->object_meta($this->media_bucket, $object);
				//文件大小不一致，重新上传
				if($object_info && $object_info['size'] != $filesize){
					if($filesize <= C("oss_100K_size")){
						//不使用分片
						$object = oss_object($subfix);
						$media_data = array();
						$media_data['id'] = $media_id;
						$media_data['object'] = $object;
						$media_data['upload_id'] = '';
						$media_data['status'] = 0;
						$media_data['size'] = $filesize;
						$meida_res = $model->save($media_data);
						$sign_url = $oss_obj->upload_sign_uri($object, $this->media_bucket);
						$parts = array();
						$parts[] = array(
							'partNumber'	=> 1,
							'seekTo'		=> 0,
							'length'		=> intval($filesize),
							'url'			=> $sign_url
						);
						$result = array(
							'name'	=> $filepath,
							'md5'	=> $filemd5,
							'key'	=> $object,
							'parts'	=> $parts
						);
					}else{
						//文件分片
						$part_size = $oss_obj->part_size($filesize);
						$upload_parts = $oss_obj->generate_upload_part($filesize, $part_size);
						//获取uploadId
						$upload_info = $oss_obj->get_upload_id($subfix, $this->media_bucket);
						//上传文件信息入库
						$object = $upload_info['Key'];
						$uploadId = $upload_info['UploadId'];
						$media_data = array();
						$media_data['id'] = $media_id;
						$media_data['object'] = $object;
						$media_data['upload_id'] = $uploadId;
						$media_data['status'] = 0;
						$media_data['size'] = $filesize;
						$meida_res = $model->save($media_data);
						//获取每片文件签名地址
						$parts = array();
						foreach($upload_parts as $key=>$val){
							$part_number = $key+1;
							$sign_url = $oss_obj->upload_part_sign($object, $uploadId, $part_number, $this->media_bucket);
							$parts[] = array(
								'partNumber'	=> $part_number,
								'seekTo'		=> $val['seekTo'],
								'length'		=> $val['length'],
								'url'			=> $sign_url
							);
						}
						if($parts){
							$result = array(
								'name'	=> $filepath,
								'md5'	=> $filemd5,
								'key'	=> $object,
								'parts'	=> $parts
							);
						}
					}
				}
			}
		}else{
			//文件不存在
			if($filesize <= C("oss_100K_size")){
				//不使用分片
				$object = oss_object($subfix);
				$media_data = array();
				$media_data['user_id'] = $user_id;
				$media_data['name'] = mysql_escape_string($filename);
				//$media_data['name'] = $filename;
				$media_data['object'] = $object;
				$media_data['upload_id'] = '';
				$media_data['md5'] = $filemd5;
				$media_data['size'] = $filesize;
				$media_data['publish'] = NOW_TIME;
				$meida_id = $model->add($media_data);
				$sign_url = $oss_obj->upload_sign_uri($object, $this->media_bucket);
				$parts = array();
				$parts[] = array(
					'partNumber'	=> 1,
					'seekTo'		=> 0,
					'length'		=> intval($filesize),
					'url'			=> $sign_url
				);
				$result = array(
					//'name'	=> $filename,
					//'path'	=> $filepath,
					'name'	=> $filepath,
					'md5'	=> $filemd5,
					'key'	=> $object,
					'parts'	=> $parts
				);
			}else{
				//文件分片
				$part_size = $oss_obj->part_size($filesize);
				$upload_parts = $oss_obj->generate_upload_part($filesize, $part_size);
				//获取uploadId
				$upload_info = $oss_obj->get_upload_id($subfix, $this->media_bucket);
				//上传文件信息入库
				$object = $upload_info['Key'];
				$uploadId = $upload_info['UploadId'];
				$media_data = array();
				$media_data['user_id'] = $user_id;
				$media_data['name'] = mysql_escape_string($filename);
				//$media_data['name'] = $filename;
				$media_data['object'] = $object;
				$media_data['upload_id'] = $uploadId;
				$media_data['md5'] = $filemd5;
				$media_data['size'] = $filesize;
				$media_data['publish'] = NOW_TIME;
				$meida_id = $model->add($media_data);
				//获取每片文件签名地址
				$parts = array();
				foreach($upload_parts as $key=>$val){
					$part_number = $key+1;
					$sign_url = $oss_obj->upload_part_sign($object, $uploadId, $part_number, $this->media_bucket);
					$parts[] = array(
						'partNumber'	=> $part_number,
						'seekTo'		=> $val['seekTo'],
						'length'		=> $val['length'],
						'url'			=> $sign_url
					);
				}
				if($parts){
					$result = array(
						//'name'	=> $filename,
						//'path'	=> $filepath,
						'name'	=> $filepath,
						'md5'	=> $filemd5,
						'key'	=> $object,
						'parts'	=> $parts
					);
				}
			}
		}
		return $result;
	}
}
