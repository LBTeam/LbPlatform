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
	private $media_bucket;
	private $program_bucket;
	public function _initialize(){
		$request = file_get_contents('php://input');
		$this->param = json_decode($request, true);
		$this->param = array(
			'Id' => "R3dAUyFk",
			'Key' => 'nKpYrjsx5UdPuMS4',
			'Mac' => '4C-CC-6A-05-70-7B'
		);
		if(empty($this->param) === true){
			$respones = array('err_code'=>'010001', 'msg'=>"Protocol content error");
			$this->ajaxReturn($respones);exit;
		}
		$this->media_bucket = C("oss_media_bucket");
		$this->program_bucket = C("oss_program_bucket");
	}
	
	public function index(){
		
	}
	
	/**
	 * 绑定屏幕和播放器
	 */
	public function bind_player(){
		$obj = $this->param;
		$bind_id	= $obj['Id'];
		$bind_key	= $obj['Key'];
		$mac		= strtoupper(str_replace(':', '-', $obj['Mac']));
		$player_model = D("Player");
		$player = $player_model->player_by_bind($bind_id, $bind_key, "id,mac");
		if($player){
			if($mac == $player['mac']){
				$respones = array("err_code"=>"020102","msg"=>"Binding player repeat");
			}else{
				$map = array('id'=>$player['id']);
				$data = array('mac'=>$mac);
				$res = $player_model->where($map)->save($data);
				if($res){
					$respones = array("err_code"=>"000000","msg"=>"ok","data"=>array("screen_id"=>$player['id']));
				}else{
					$respones = array("err_code"=>"020103","msg"=>"Binding player failed");
				}
			}
		}else{
			$respones = array("err_code"=>"020101","msg"=>"Binding parameter error");
		}
		$this->ajaxReturn($respones);
	}
	
	/**
	 * 心跳
	 */
	public function heartbeat(){
		$obj = $this->param;
		$id = $obj['Id'];
		$key = $obj['Key'];
		$mac = strtoupper(str_replace(':', '-', $obj['Mac']));
		$player_model = D("Player");
		$player = $player_model->player_by_bind($id, $key, "id,mac");
		if($mac == $player['mac']){
			$led_id = $player['id'];
		
			//testing start
			$led_id = 1;
			//testing end
			
			$screen_model = D("Screen");
			$cmd_model = D("Command");
			$plan_model = D("Program");
			$media_model = D("Media");
			$AliyunOSS = new AliyunOSS();
			$screen = $screen_model->screen_by_id($led_id);
			$cmds_list = $cmd_model->cmds_list($screen['uid'], $led_id);
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
							$media_list = json_decode($plan['info'], true);
							foreach($media_list as $v){
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
			$cmd_model->cmd_issued($cmd_ids);
			$respones = array(
				"err_code"=>"000000",
				"msg"=>"ok",
				"Id"=>$id,
				"Key"=>$key,
				"Mac"=>$mac,
				"data"=>$cmds
			);
		}else{
			$respones = array("err_code"=>"020201","msg"=>"Player MAC error");
		}
		$this->ajaxReturn($respones);
	}
	
	/**
	 * 监控数据上传
	 * CPU使用率		cpu usage
	 * 硬盘使用率	disk usage
	 * 内存使用率	memory usage
	 * CPU温度		cpu temperature
	 * 风扇转速		fan speed
	 */
	public function alarm(){
		$obj = $this->param;
		$bind_id	= $obj['Id'];
		$bind_key	= $obj['Key'];
		$mac		= strtoupper(str_replace(':', '-', $obj['Mac']));
		$player_model = D("Player");
		$player = $player_model->player_by_bind($id, $key, "id,mac");
		if($mac == $player['mac']){
			$screen_id = $player['id'];
			$alarm_model = D("Alarm");
			$alarm = $alarm_model->alarm_by_sid($screen_id);
		}else{
			$respones = array("err_code"=>"020201","msg"=>"Player MAC error");
		}
		$this->ajaxReturn($respones);
	}
	

	public function screen(){
		$screen_model = D("Screen");
		$screen = $screen_model->screen_by_id(1);
		$regions = D("Region")->all_region_name();
		$province = $regions[$screen['province']];
		$city = $regions[$screen['city']];
		$district = $regions[$screen['district']];
		$temp = array(
			'id'		=> $screen['id'],
			'name'		=> $screen['name'],
			'size_x'	=> intval($screen['size_x']),
			'size_y'	=> intval($screen['size_y']),
			'resolu_x'	=> intval($screen['resolu_x']),
			'resolu_y'	=> intval($screen['resolu_y']),
			'type'		=> intval($screen['type']),
			'operate'	=> intval($screen['operate']),
			'longitude'	=> floatval($screen['longitude']),
			'latitude'	=> floatval($screen['latitude']),
			'address'	=> "{$province}{$city}{$district}{$screen['address']}",
		);
		$this->ajaxReturn($temp);
	}
}
