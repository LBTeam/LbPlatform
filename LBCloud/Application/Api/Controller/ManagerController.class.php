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
		
		$this->param = json_decode($request);
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
	 * 文件分片
	 */
	public function upload_part(){
		$total = $this->param->total;
		$part = $this->param->part;
		$total = 2014568;
		$part = 102400;
		$AliyunOSS = new AliyunOSS();
		$result = $AliyunOSS->generate_upload_part($total, $part);
		$response = ["err_code"=>"000000", "msg"=>"ok", 'data'=>$result];
		$this->ajaxReturn($response);
	}
	
	/**
	 * 获取uploadId
	 */
	public function upload_id(){
		$AliyunOSS = new AliyunOSS();
		$name = $this->param->name;
		$array = explode('.', $name);
		$subfix = end($array);
		$md5 = $this->param->md5;
		$bucket = C("aliyun_oss_bucket");
		$result = $AliyunOSS->get_upload_id($subfix, $bucket);
		$response = ["err_code"=>"000000", "msg"=>"ok", 'data'=>$result];
		$this->ajaxReturn($response);
	}
	
	/**
	 * 分片上传地址
	 */
	public function part_sign_url(){
		$AliyunOSS = new AliyunOSS();
		$object = $this->param->Key;
		$uploadId = $this->param->UploadId;
		$part = $this->param->partNumber;
		$md5 = $this->param->md5;
		$uri = $AliyunOSS->upload_part_sign($object, $uploadId, $part, $md5);
		$response = ["err_code"=>"000000", "msg"=>"ok", 'data'=>['url'=>$uri]];
		$this->ajaxReturn($response);
	}
	
	public function upload(){
		$result = [];
		$obj = $this->param;
		foreach ($obj as $val) {
			if(true){
				//文件不存在
				
				//文件分片
				//获取uploadId
				//上传文件信息入库
				//获取每片文件签名地址
			}else{
				//文件存在
				if(true){
					//已上传完成
					//不做处理
				}else{
					//未上传完成
					//获取oss端上传成功的分片
					//对比得出还需要上传的分片及上传地址等
				}
			}
		}
		
	}
	
	public function demo1(){
		$string = '"[{\"FilePath\":\"aaa\",\"FileSize\":\"222\",\"FileMD5\":\"333\"},{\"FilePath\":\"aaa\",\"FileSize\":\"222\",\"FileMD5\":\"333\"}]"';
		$string = json_decode(json_decode($string, true), true);
		dump($string);
	}
	
	public function demo(){
		//$request = file_get_contents('php://input');
		//file_put_contents('./1.log', json_encode($request));
		$result = [];
		$part = [
			['partNumber'=>1, 'seekTo'=>0, 'length'=> 102400, 'url'=>'http://aliyunoss.com/xxxxxxxxxxxxx'],
			['partNumber'=>2, 'seekTo'=>102400, 'length'=> 102400, 'url'=>'http://aliyunoss.com/xxxxxxxxxxxxx'],
			['partNumber'=>3, 'seekTo'=>204800, 'length'=> 102400, 'url'=>'http://aliyunoss.com/xxxxxxxxxxxxx'],
			['partNumber'=>4, 'seekTo'=>307200, 'length'=> 102400, 'url'=>'http://aliyunoss.com/xxxxxxxxxxxxx'],
			['partNumber'=>5, 'seekTo'=>409600, 'length'=> 102400, 'url'=>'http://aliyunoss.com/xxxxxxxxxxxxx'],
			['partNumber'=>6, 'seekTo'=>512000, 'length'=> 68968, 'url'=>'http://aliyunoss.com/xxxxxxxxxxxxx']
		];
		$result = [
			['name'=>"123.avi",'key'=>'57a31bb736fda.avi','parts'=>$part],
			['name'=>"123.avi",'key'=>'57a31bb736fda.avi','parts'=>$part],
			['name'=>"123.avi",'key'=>'57a31bb736fda.avi','parts'=>$part]
		];		
		echo json_encode($result);
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
