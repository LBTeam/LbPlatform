<?php
/**
 * 播放端接口控制器
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Api\Controller;
use Api\Service\AliyunOSS;

class PlayerController extends CommonController
{
	private $param;
	private $user_id;
	private $media_bucket;
	private $program_bucket;
	public function _initialize(){
		$request = file_get_contents('php://input');
		$this->param = json_decode($request, true);
		$this->user_id = 1;
		$this->media_bucket = C("oss_media_bucket");
		$this->program_bucket = C("oss_program_bucket");
	}
	
	public function index(){
		
	}
	
	/**
	 * 绑定屏幕和播放器
	 */
	public function bind_player(){
		
	}
	
	/**
	 * 心跳
	 */
	public function heartbeat(){
		
	}
	
	/**
	 * 下载地址
	 */
	public function download_url(){
		$obj = $this->param;
		$response = [];
		$media_model = D("Media");
		$program_model = D("Program");
		$AliyunOSS = new AliyunOSS();
		foreach($obj as $val){
			$filename = $val['FileName'];
			$filemd5 = $val['FileMD5'];
			$filesubfix = end(explode('.', $filename));
			if($filesubfix == C('player_program_subfix')){
				//播放方案
				$fileinfo = $program_model->program_by_name_md5($filename, $filemd5, $this->user_id);
				$bucket = $this->program_bucket;
			}else{
				//媒体
				$fileinfo = $media_model->media_by_name_md5($filename, $filemd5, $this->user_id);
				$bucket = $this->media_bucket;
			}
			if($fileinfo){
				$object = $fileinfo['object'];
				$request_uri = $AliyunOSS->download_uri($bucket, $object, 1800);
				$response[] = [
					'name'	=>	$filename,
					'md5'	=>	$filemd5,
					'key'	=>	$object,
					'url'	=>	$request_uri
				];
			}else{
				continue;
			}
		}
		$this->ajaxReturn($response);
	}
}
