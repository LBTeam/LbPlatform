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
	public function _initialize(){
		$request = file_get_contents('php://input');
		$token = I("request.token");
		
		$proObj = json_decode($request);
		$this->param = $proObj->Para;
	}
	
	public function index(){
		$request = file_get_contents('php://input');
		$token = I("request.token");
		
		$proObj = json_decode($request);
		$proId = $proObj->Id;
		$param = $proObj->Para;
		
		//testing start
		$proId = '010100';
		//testing end
		
		$response = array();
		
		if(empty($proId)){
			$response = array("err_code"=>"010001", "msg"=>"内部协议号错误~!");
		}
		
		switch($proId){
			case '010100':
				//oss配置
				break;
			case '010101':
				//登录
				break;
			case '010102':
				//刷新token
				break;
			case '010103':
				//媒体文件分片
				$AliyunOSS = new AliyunOSS();
				$total = $param->total;
				$part = $param->part;
				$result = $AliyunOSS->generate_upload_part($total, $part);
				$response = array("err_code"=>"000000", "msg"=>"ok", 'data'=>$result);
				break;
			case '010104':
				//获取上传UploadId
				$AliyunOSS = new AliyunOSS();
				$name = $param->name;
				$array = explode('.', $name);
				$subfix = end($array);
				$md5 = $param->md5;
				$bucket = C("aliyun_oss_bucket");
				$result = $AliyunOSS->get_upload_id($subfix, $bucket);
				$response = array("err_code"=>"000000", "msg"=>"ok", 'data'=>$result);
				break;
			case '010105':
				//获取分片上传签名地址
				$AliyunOSS = new AliyunOSS();
				$object = $param->Key;
				$uploadId = $param->UploadId;
				$part = $param->partNumber;
				$md5 = $param->md5;
				$uri = $AliyunOSS->upload_part_sign($object, $uploadId, $part, $md5);
				$response = array("err_code"=>"000000", "msg"=>"ok", 'data'=>['url'=>$uri]);
				break;
			case '010106':
				//完成上传
				$AliyunOSS = new AliyunOSS();
				$object = $param->Key;
				$uploadId = $param->UploadId;
				$parts = $param->uploadParts;
				$result = $AliyunOSS->complete_upload($object, $uploadId, $parts);
				$response = array("err_code"=>"000000", "msg"=>"ok", 'data'=>[]);
				break;
			case '010107':
				//中止上传
				break;
			case '010108':
				//查询媒体文件是否存在
				break;
			case '010109':
				//获取播放方案列表
				break;
			case '010110':
				//获取播放方案详情
				break;
			case '010111':
				//获取媒体列表
				break;
			case '010112':
				//获取媒体详情
				break;
			case '010113':
				//获取终端列表
				break;
			case '010114':
				//发布播放方案
				break;
			case '010115':
				//备份播放方案
				break;
			default:
				//登录
				break;
		}
		$this->ajaxReturn($response);
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
