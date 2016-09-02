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
		if(empty($this->param) === true){
			$response = array('err_code'=>'010001', 'msg'=>"Protocol content error");
			$this->ajaxReturn($response);exit;
		}
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
		$obj = $this->param;
		$id = $obj['Id'];
		//testing start
		$id = 1;
		//testing end
		$key = $obj['Key'];
		$mac = strtoupper(str_replace(':', '-', $obj['Mac']));
		$screen_model = D("Screen");
		$cmd_model = D("Command");
		$plan_model = D("Program");
		$media_model = D("Media");
		$AliyunOSS = new AliyunOSS();
		$screen = $screen_model->screen_by_id($id);
		$cmds_list = $cmd_model->cmds_list($screen['uid'], $id);
		$cmds = array();
		$cmd_ids = array();
		foreach($cmds_list as $val){
			$cmd_ids[] = $val['id'];
			switch($val['type']){
				case "0":
					$param = json_decode($val['param'], true);
					$plan = $plan_model->program_detail($param['program_id']);
					if($plan){
						$medias = array();
						foreach($plan['info'] as $v){
							$media = $media_model->media_by_name_md5($v['MediaName'], $v['MediaMD5'], $screen['uid']);
							$medias[] = array(
								'MediaId'	=> $media['id'],
								'MediaName'	=> stripslashes($media['name']),
								'MediaUrl'	=> $AliyunOSS->download_uri($this->media_bucket, $media['object'])
							);
						}
						$cmdParam = array(
							'ProgramId'		=> $plan['id'],
							'ProgramName'	=> stripslashes($plan['name']),
							'ProgramUrl'	=> $AliyunOSS->download_uri($this->program_bucket, $plan['object']),
							'Medias'		=> $medias
						);
						$cmds[] = array(
							"CmdId"		=>	$val['id'],
							"CmdType"	=>	intval($val['type']),
							"CmdParam"	=>	base64_encode(json_encode($cmdParam))
						);
					}
					break;
				case "1":
					break;
				default:
					break;
			}
		}
		//更改命令已下发
		//$cmd_model->cmd_issued($cmd_ids);
		$response = array(
			"err_code"=>"000000",
			"msg"=>"ok",
			"Id"=>$id,
			"Key"=>$key,
			"Mac"=>$mac,
			"data"=>$cmds
		);
		$this->ajaxReturn($response);
	}
}
