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
		/*$token = I("request.token");
		$this->user_model = D("User");
		if(in_array(ACTION_NAME, C("manager_not_logged"))){
			//未登录可访问模块
			//不检查登录令牌
			
		}else{
			//检查token
			if($token){
				$this->user_id = $this->user_model->check_token($token);
				if($this->user_id === false){
					$response = array('err_code'=>'010002', 'msg'=>"Token error");
					$this->ajaxReturn($response);exit;
				}else{
					if(!$this->user_id){
						$response = array('err_code'=>'010003', 'msg'=>"Token timeout");
						$this->ajaxReturn($response);exit;
					}
				}
			}else{
				$response = array('err_code'=>'010002', 'msg'=>"Token error");
				$this->ajaxReturn($response);exit;
			}
		}*/
		$this->user_id = 1;
		/*登录令牌（token）检测结束*/
		$this->param = json_decode($request, true);
		/*请求数据检测开始*/
		if(in_array(ACTION_NAME, C("manager_param_empty"))){
			//空请求数据可访问模块
			
		}else{
			//检查请求数据
			if(empty($this->param) === true){
				$response = array('err_code'=>'010001', 'msg'=>"Protocol content error");
				$this->ajaxReturn($response);exit;
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
		$response = array("err_code"=>"000000", "msg"=>"ok", 'data'=>$configure);
		$this->ajaxReturn($response);
	}

	/**
	 * 登录
	 */
	public function login(){
		$obj = $this->param;
		$username = $obj['user'];
		$password = $obj['pwd'];
		$response = array();
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
					$response = array('err_code'=>'000000', 'msg'=>"ok", 'data'=>$return);
				}else{
					$response = array('err_code'=>'010103', 'msg'=>"Login failed");
				}
			}else{
				$response = array('err_code'=>'010102', 'msg'=>"Password error");
			}
		}else{
			$response = array('err_code'=>'010101', 'msg'=>"User does not exist");
		}
		$this->ajaxReturn($response);
	}
	
	/**
	 * 刷新token
	 */
	public function refresh_token(){
		
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
		foreach ($obj as $val) {
			$user_id = $this->user_id;
			$filepath = $val['FilePath'];
			$filename = end(explode('/', str_replace('\\', '/', $filepath)));
			$filesize = $val['FileSize'];
			$filemd5 = $val['FileMD5'];
			$filesubfix = end(explode('.', $filename));
			$filetype = $val['Type'];   //0-播放方案,1-图片,2-视频,3-文字
			if($filetype == 0){
				//播放方案
				$medias = $val['MediaList'];
				foreach($medias as &$v){
					$v['MediaName'] = end(explode('/', str_replace('\\', '/', $v['MediaName'])));
				}
				$program_data = $this->_upload_program($program_model, $AliyunOSS, $user_id, $filename, $filesize, $filemd5, $filesubfix, $medias, $filepath);
				if($program_data){
					$program_data['type'] = $filetype;
					$result[] = $program_data;
				}
			}else{
				//媒体
				$media_data = $this->_upload_media($media_model, $AliyunOSS, $user_id, $filename, $filesize, $filemd5, $filesubfix, $filepath);
				//file_put_contents('./1.log', json_encode($media_data)."\r\n", FILE_APPEND);
				if($media_data){
					$media_data['type'] = $filetype;
					$result[] = $media_data;
				}
			}
		}
		//file_put_contents('./1.log', json_encode($result)."\r\n", FILE_APPEND);
		$this->ajaxReturn($result);
	}
	
	/**
	 * 完成上传
	 */
	public function complete_upload(){
		$obj = $this->param;
		//$filename = $obj['FileName'];
		$filepath = $obj['FileName'];
		$filename = end(explode('/', str_replace('\\', '/', $filepath)));
		$filemd5 = $obj['FileMD5'];
		$filetype = $obj['FileType'];
		$fileparts = $obj['Parts'];
		$user_id = $this->user_id;
		if($filetype == 0){
			//播放方案
			$screens = $obj['Screens'];
			$prog_model = D("Program");
			$prog_info = $prog_model->program_by_name_md5($filename, $filemd5, $user_id);
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
			//播放方案下发
			$cmds = array();
			foreach($screens as $val){
				$cmds[] = array(
					'user_id'	=> $user_id,
					'screen_id'	=> $val,
					'type'		=> 0,
					'param'		=> json_encode(array('program_id'=>$prog_info['id'])),
					'publish'	=> NOW_TIME,
					'execute'	=> NOW_TIME,
					'expired'	=> NOW_TIME,
					'status'	=> 0
				);
			}
			if($cmds){
				$cmd_model = D("Command");
				$cmd_del = $cmd_model->remove_cmd($user_id, $screens, 0, 0);
				$cmd_add = $cmd_model->release_cmd($cmds);
			}
		}else{
			//媒体
			$media_model = D("Media");
			$media_info = $media_model->media_by_name_md5($filename, $filemd5, $user_id);
			$media_map = array('id'=>$media_info['id']);
			$media_res = $media_model->where($media_map)->setField('status', 1);
			if($media_info['size'] >= C("oss_100K_size")){
				$AliyunOSS = new AliyunOSS();
				//合并文件
				$object = $media_info['object'];
				$uploadId = $media_info['upload_id'];
				$parts = array();
				foreach($fileparts as $val){
					$parts[] = array('PartNumber'=>$val['partNumber'],'ETag'=>strtoupper($val['MD5']));
				}
				$oss_res = $AliyunOSS->complete_upload($object, $uploadId, $parts, $this->media_bucket);
			}
		}
		$response = array("err_code"=>"000000","msg"=>"success");
		$this->ajaxReturn($response);
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
		$response = array();
		$response['groups'] = array_values($groups);
		$response['screens'] = $screens;
		$this->ajaxReturn($response);
	}

	/**
	 * 发布方案
	 */
	public function publish(){
		$obj = $this->param;
		$groups = $obj['groups'];
		$screens = $obj['screens'];
		$program = $obj['program'];
		$user_id = $this->user_id;
		$program_model = D("Program");
		$release = $program_model->program_can_release($program, $user_id);
		if($release){
			$group_model = D("Group");
			$g_screens = $group_model->group_screens($groups, $user_id);
			$all_screen = array_unique(array_merge($g_screens, $screens));
			
			
		}else{
			$response = array('err_code'=>'010110', 'msg'=>"Program is not complete, can not be released");
		}
		$this->ajaxReturn($response);
	}
	
	/**
	 * 播放方案列表
	 */
	public function programs(){
		$user_id = $this->user_id;
		$program_model = D("Program");
		$media_model = D("Media");
		$AliyunOSS = new AliyunOSS();
		$all_program = $program_model->all_program($user_id);
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
	 * 媒体列表
	 */
	public function medias(){
		
	}
	
	/**
	 * 上传播放方案
	 */
	private function _upload_program($model, $oss_obj, $user_id, $filename, $filesize, $filemd5, $subfix, $medias, $filepath){
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
						'length'		=> $filesize,
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
				$program_data['name'] = mysql_real_escape_string($filename);
				$program_data['object'] = $object;
				$program_data['upload_id'] = '';
				$program_data['info'] = json_encode($medias);
				$program_data['md5'] = $filemd5;
				$program_data['size'] = $filesize;
				$program_data['publish'] = NOW_TIME;
				$program_id = $model->add($program_data);
				$sign_url = $oss_obj->upload_sign_uri($object, $this->program_bucket);
				$parts = array();
				$parts[] = array(
					'partNumber'	=> 1,
					'seekTo'		=> 0,
					'length'		=> $filesize,
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
				$program_data['name'] = mysql_real_escape_string($filename);
				$program_data['object'] = $object;
				$program_data['upload_id'] = $uploadId;
				$program_data['info'] = json_encode($medias);
				$program_data['md5'] = $filemd5;
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
			if($media_info['status'] == 0){
				$object = $media_info['object'];
				$uploadId = $media_info['upload_id'];
				if($filesize <= C("oss_100K_size")){
					//不使用分片
					$sign_url = $oss_obj->upload_sign_uri($object, $this->media_bucket);
					$parts = array();
					$parts[] = array(
						'partNumber'	=> 1,
						'seekTo'		=> 0,
						'length'		=> $filesize,
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
				//不做处理
			}
		}else{
			//文件不存在
			if($filesize <= C("oss_100K_size")){
				//不使用分片
				$object = oss_object($subfix);
				$media_data = array();
				$media_data['user_id'] = $user_id;
				$media_data['name'] = mysql_real_escape_string($filename);
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
					'length'		=> $filesize,
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
				$media_data['name'] = mysql_real_escape_string($filename);
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
