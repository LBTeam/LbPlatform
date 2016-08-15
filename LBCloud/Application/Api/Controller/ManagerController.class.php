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
	private $media_bucket;
	private $program_bucket;
	public function _initialize(){
		$request = file_get_contents('php://input');
		//$request = '[{"FilePath":"aabbccdd.playprog","FileSize":"2097152","FileMD5":"586af24095a05643c3be4bb402bfaee5"},{"FilePath":"aabbcc.avi","FileSize":"20971520","FileMD5":"b3206b4529ba377b0fa9f4a3bd9261f2"}]';
		$token = I("request.token");
		
		//$this->param = json_decode($request, true);
		$this->param = json_decode($request);
		$this->user_id = 1;
		$this->media_bucket = C("oss_media_bucket");
		$this->program_bucket = C("oss_program_bucket");
	}
	
	/**
	 * oss配置
	 */
	public function configuration(){
		$configure = [];
		$configure['accessKeyId']		= C("aliyun_oss_id");
		$configure['accessKeySecret']	= C("aliyun_oss_secret");
		$configure['endpoint']			= C("aliyun_oss_endpoint");
		$configure['bucket']			= C("aliyun_oss_bucket");
		$configure['mediaBucket']		= C("oss_media_bucket");
		$configure['programBucket']		= C("oss_program_bucket");
		$configure = encrypt(json_encode($configure));
		$response = ["err_code"=>"000000", "msg"=>"ok", 'data'=>$configure];
		$this->ajaxReturn($response);
	}

	/**
	 * 登录
	 */
	public function login(){
		
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
		$result = [];
		$obj = $this->param;
		$program_model = D("Program");
		$media_model = D("Media");
		$AliyunOSS = new AliyunOSS();
		foreach ($obj as $val) {
			$user_id = $this->user_id;
			$filename = $val['FilePath'];
			$filesize = $val['FileSize'];
			$filemd5 = $val['FileMD5'];
			$filesubfix = end(explode('.', $filename));
			$part_size = C("oss_part_size");
			if($filesubfix == C('player_program_subfix')){
				//播放方案
				$program_id = $program_model->program_exists($filename, $filemd5, $user_id);
				if($program_id){
					//文件存在
					$program_info = $program_model->program_detail($program_id);
					if($program_info['status'] == 0){
						//未上传完成
						//获取oss端上传成功的分片
						//对比得出还需要上传的分片及上传地址等
						$upload_parts = $AliyunOSS->generate_upload_part($filesize, $part_size);
						$object = $program_info['object'];
						$uploadId = $program_info['upload_id'];
						$part_lists = $AliyunOSS->part_list($object, $uploadId, $this->program_bucket);
						$part_list = [];
						if($part_lists){
							foreach($part_lists as $val){
								$part_list[] = $val['partNumber'];
							}
						}
						$parts = [];
						foreach($upload_parts as $key=>$val){
							$part_number = $key+1;
							if(!in_array($part_number, $part_list)){
								//需要上传的分片
								$sign_url = $AliyunOSS->upload_part_sign($object, $uploadId, $part_number, $this->program_bucket);
								$parts[] = [
									'partNumber'	=> $part_number,
									'seekTo'		=> $val['seekTo'],
									'length'		=> $val['length'],
									'url'			=> $sign_url
								];
							}
						}
						if($parts){
							$result[] = [
								'name'	=> $filename,
								'key'	=> $object,
								'parts'	=> $parts
							];
						}
					}else{
						//已上传完成
						//不做处理
					}
				}else{
					//文件不存在
					//文件分片
					$upload_parts = $AliyunOSS->generate_upload_part($filesize, $part_size);
					//获取uploadId
					$upload_info = $AliyunOSS->get_upload_id($filesubfix, $this->program_bucket);
					//上传文件信息入库
					$object = $upload_info['Key'];
					$uploadId = $upload_info['UploadId'];
					$program_data = [];
					$program_data['user_id'] = $this->user_id;
					$program_data['name'] = $filename;
					$program_data['object'] = $object;
					$program_data['upload_id'] = $uploadId;
					$program_data['md5'] = $filemd5;
					$program_data['size'] = $filesize;
					$program_data['publish'] = NOW_TIME;
					$program_id = $program_model->add($program_data);
					//获取每片文件签名地址
					$parts = [];
					foreach($upload_parts as $key=>$val){
						$part_number = $key+1;
						$sign_url = $AliyunOSS->upload_part_sign($object, $uploadId, $part_number, $this->program_bucket);
						$parts[] = [
							'partNumber'	=> $part_number,
							'seekTo'		=> $val['seekTo'],
							'length'		=> $val['length'],
							'url'			=> $sign_url
						];
					}
					if($parts){
						$result[] = [
							'name'	=> $filename,
							'key'	=> $object,
							'parts'	=> $parts
						];
					}
				}
			}else{
				//媒体
				$media_id = $media_model->media_exists($filename, $filemd5, $user_id);
				if($media_id){
					$media_info = $media_model->media_detail($media_id);
					if($media_info['status'] == 0){
						$upload_parts = $AliyunOSS->generate_upload_part($filesize, $part_size);
						$object = $media_info['object'];
						$uploadId = $media_info['upload_id'];
						$part_lists = $AliyunOSS->part_list($object, $uploadId, $this->media_bucket);
						$part_list = [];
						foreach($part_lists as $val){
							$part_list[] = $val['partNumber'];
						}
						$parts = [];
						foreach($upload_parts as $key=>$val){
							$part_number = $key+1;
							if(!in_array($part_number, $part_list)){
								//需要上传的分片
								$sign_url = $AliyunOSS->upload_part_sign($object, $uploadId, $part_number, $this->media_bucket);
								$parts[] = [
									'partNumber'	=> $part_number,
									'seekTo'		=> $val['seekTo'],
									'length'		=> $val['length'],
									'url'			=> $sign_url
								];
							}
						}
						if($parts){
							$result[] = [
								'name'	=> $filename,
								'key'	=> $object,
								'parts'	=> $parts
							];
						}
					}else{
						//已上传完成
						//不做处理
					}
				}else{
					//文件不存在
					//文件分片
					$upload_parts = $AliyunOSS->generate_upload_part($filesize, $part_size);
					//获取uploadId
					$upload_info = $AliyunOSS->get_upload_id($filesubfix, $this->media_bucket);
					//上传文件信息入库
					$object = $upload_info['Key'];
					$uploadId = $upload_info['UploadId'];
					$media_data = [];
					$media_data['user_id'] = $this->user_id;
					$media_data['name'] = $filename;
					$media_data['object'] = $object;
					$media_data['upload_id'] = $uploadId;
					$media_data['md5'] = $filemd5;
					$media_data['size'] = $filesize;
					$media_data['publish'] = NOW_TIME;
					$meida_id = $media_model->add($media_data);
					//获取每片文件签名地址
					$parts = [];
					foreach($upload_parts as $key=>$val){
						$part_number = $key+1;
						$sign_url = $AliyunOSS->upload_part_sign($object, $uploadId, $part_number, $this->media_bucket);
						$parts[] = [
							'partNumber'	=> $part_number,
							'seekTo'		=> $val['seekTo'],
							'length'		=> $val['length'],
							'url'			=> $sign_url
						];
					}
					if($parts){
						$result[] = [
							'name'	=> $filename,
							'key'	=> $object,
							'parts'	=> $parts
						];
					}
				}
			}
		}
		$this->ajaxReturn($result);
	}
	
	/**
	 * 完成上传
	 */
	public function complete_upload(){
		$AliyunOSS = new AliyunOSS();
		$object = $this->param->Key;
		$uploadId = $this->param->UploadId;
		$parts = $this->param->uploadParts;
		$result = $AliyunOSS->complete_upload($object, $uploadId, $parts);
		$response = ["err_code"=>"000000", "msg"=>"ok", 'data'=>[]];
		$this->ajaxReturn($response);
	}
	
	/**
	 * 终端列表
	 */
	public function screens(){
		$user_id = $this->param->UserId;
		$user_id = 1;
		$screen_model = D("Screen");
		$result = $screen_model->user_all_screen($user_id);
		$regions = D("Region")->all_region_name();
		$screens = [];
		$groups = [];
		foreach($result as $val){
			$province = $regions[$val['province']];
			$city = $regions[$val['city']];
			$district = $regions[$val['district']];
			if(is_null($val['group_id'])){
				$screens[] = [
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
				];
			}else{
				if(!$groups[$val['group_id']]){
					$groups[$val['group_id']] = [
						'id'		=> $val['group_id'],
						'name'		=> $val['group_name']
					];
				}
				$groups[$val['group_id']]['screens'][] = [
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
				];
			}
		}
		$list = [];
		$list['groups'] = array_values($groups);
		$list['screens'] = $screens;
		$list = encrypt(json_encode($list));
		$response = ["err_code"=>"000000", "msg"=>"ok", 'data'=>$list];
		$this->ajaxReturn($response);
	}
	
	/**
	 * 播放方案列表
	 */
	public function programs(){
		
	}
	
	/**
	 * 媒体列表
	 */
	public function medias(){
		
	}
}
