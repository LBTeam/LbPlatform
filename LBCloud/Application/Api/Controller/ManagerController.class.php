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
		/*$array = array(
			array("FilePath"=>"aabb'cc.playprog", "FileSize"=>"102400", "FileMD5"=>"586af24095a05643c3be4bb402dsdwqs"),
			array("FilePath"=>"aabbccdd.playprog", "FileSize"=>"2097152", "FileMD5"=>"586af24095a05643c3be4bb402bfaee5"),
			array("FilePath"=>"aab'bcc.avi", "FileSize"=>"20971520", "FileMD5"=>"b3206b4529ba377b0fa9f4a3bd9261f2"),
		);
		$request = json_encode($array);*/
		//$request = '{"user":"15934854815@163.com","pwd":"123456"}';
		/*登录令牌（token）检测开始*/
		/*$token = I("request.token");
		$this->user_model = D("User");
		if(in_array(ACTION_NAME, C("not_logged_allow"))){
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
		/*if(!$this->param){
			$response = array('err_code'=>'010001', 'msg'=>"Protocol content error");
			$this->ajaxReturn($response);exit;
		}*/
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
		$configure = encrypt(json_encode($configure));
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
			$filename = end(explode('/', str_replace('\\', '/', $val['FilePath'])));
			$filesize = $val['FileSize'];
			$filemd5 = $val['FileMD5'];
			$filesubfix = end(explode('.', $filename));
			if($filesubfix == C('player_program_subfix')){
				//播放方案
				$medias = $val['MediaList'];
				$program_data = $this->_upload_program($program_model, $AliyunOSS, $user_id, $filename, $filesize, $filemd5, $filesubfix, $medias);
				if($program_data){
					$result[] = $program_data;
				}
			}else{
				//媒体
				$media_data = $this->_upload_media($media_model, $AliyunOSS, $user_id, $filename, $filesize, $filemd5, $filesubfix);
				if($media_data){
					$result[] = $media_data;
				}
			}
		}
		$this->ajaxReturn($result);
	}
	
	/**
	 * 完成上传
	 */
	public function complete_upload(){
		$obj = $this->param;
		$filename = $obj['FileName'];
		$filemd5 = $obj['FileMD5'];
		$fileparts = $obj['Parts'];
		$filesubfix = end(explode('.', $filename));
		$user_id = $this->user_id;
		if($filesubfix == C('player_program_subfix')){
			//播放方案
			$prog_model = D("Program");
			$prog_info = $prog_model->program_by_name_md5($filename, $filemd5, $user_id);
			$prog_map = array('id'=>$prog_info['id']);
			$prog_res = $prog_model->where($prog_map)->setField('status', 1);
			if($prog_info['size'] <= C("oss_100K_size")){
				$AliyunOSS = new AliyunOSS();
				//合并文件
				$object = $prog_info['object'];
				$uploadId = $prog_info['upload_id'];
				$parts = array();
				foreach($fileparts as $val){
					$parts[] = array('PartNumber'=>$val['partNumber'],'ETag' => $val['MD5']);
				}
				$oss_res = $AliyunOSS->complete_upload($object, $uploadId, $parts, $this->program_bucket);
			}
		}else{
			//媒体
			$media_model = D("Media");
			$media_info = $media_model->media_by_name_md5($filename, $filemd5, $user_id);
			$media_map = array('id'=>$media_info['id']);
			$media_res = $media_model->where($media_map)->setField('status', 1);
			if($media_info['size'] <= C("oss_100K_size")){
				$AliyunOSS = new AliyunOSS();
				//合并文件
				$object = $media_info['object'];
				$uploadId = $media_info['upload_id'];
				$parts = array();
				foreach($fileparts as $val){
					$parts[] = array('PartNumber'=>$val['partNumber'],'ETag' => $val['MD5']);
				}
				$oss_res = $AliyunOSS->complete_upload($object, $uploadId, $parts, $this->media_bucket);
			}
		}
		$response = array("err_code"=>"000000","msg"=>"success");
		$this->ajaxReturn($response);
	}

	/**
	 * 下载地址
	 */
	public function download_url(){
		$obj = $this->param;
		
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
		$groups = array();
		foreach($result as $val){
			$province = $regions[$val['province']];
			$city = $regions[$val['city']];
			$district = $regions[$val['district']];
			if(is_null($val['group_id'])){
				$screens[] = array(
					'id'		=> $val['id'],
					'name'		=> $val['name'],
					'size_x'	=> $val['size_x'],
					'size_y'	=> $val['size_y'],
					'resolu_x'	=> $val['resolu_x'],
					'resolu_y'	=> $val['resolu_y'],
					'type'		=> $val['type'],
					'operate'	=> $val['operate'],
					'longitude'	=> $val['longitude'],
					'latitude'	=> $val['latitude'],
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
					'size_x'	=> $val['size_x'],
					'size_y'	=> $val['size_y'],
					'resolu_x'	=> $val['resolu_x'],
					'resolu_y'	=> $val['resolu_y'],
					'type'		=> $val['type'],
					'operate'	=> $val['operate'],
					'longitude'	=> $val['longitude'],
					'latitude'	=> $val['latitude'],
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
			$all_screen = array_merge($g_screens, $screens);
			
			
		}else{
			$response = array('err_code'=>'010110', 'msg'=>"Program is not complete, can not be released");
		}
		$this->ajaxReturn($response);
	}
	
	/*public function demo(){
		$group_model = D("Group");
		$groups = array('1','2');
		$user_id = $this->user_id;
		$temp = $group_model->group_screens($groups, $user_id);
		dump($temp);
	}*/
	
	/**
	 * 播放方案列表
	 */
	public function programs(){
		$program_model = D("Program");
		$result = $program_model->select();
		foreach($result as &$val){
			$val['name'] = stripslashes($val['name']);
		}
		$this->ajaxReturn($result);
	}
	
	/**
	 * 媒体列表
	 */
	public function medias(){
		
	}
	
	/**
	 * 上传播放方案
	 */
	private function _upload_program($model, $oss_obj, $user_id, $filename, $filesize, $filemd5, $subfix, $medias){
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
						'name'	=> $filename,
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
							'name'	=> $filename,
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
					'name'	=> $filename,
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
						'name'	=> $filename,
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
	private function _upload_media($model, $oss_obj, $user_id, $filename, $filesize, $filemd5, $subfix){
		$result = array();
		$media_id = $model->media_exists($filename, $filemd5, $user_id);
		if($media_id){
			$media_info = $model->media_detail($media_id);
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
						'name'	=> $filename,
						'md5'	=> $filemd5,
						'key'	=> $object,
						'parts'	=> $parts
					);
				}else{
					$part_size = $oss_obj->part_size($filesize);
					$upload_parts = $oss_obj->generate_upload_part($filesize, $part_size);
					$part_lists = $oss_obj->part_list($object, $uploadId, $this->media_bucket);
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
					if($parts){
						$result = array(
							'name'	=> $filename,
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
					'name'	=> $filename,
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
						'name'	=> $filename,
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
