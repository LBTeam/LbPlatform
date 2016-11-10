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
		/*$this->param = array(
			'Id' => "deMkPdrk",
			'Key' => 'sU3PjNesZ3f4KqXg',
			'Mac' => '4C-CC-6A-05-70-7B'
		);*/
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
				//$respones = array("err_code"=>"020102","msg"=>"Binding player repeat");
				$led_model = D("Screen");
				$led_info = $led_model->screen_by_id($player['id']);
				$data = array(
					"id"		=> $bind_id,
					"key"		=> $bind_key,
					'mac'		=> $mac,
					'screen_id'	=> $player['id'],
					"name"		=> $led_info['name'],
					"size_x"	=> intval($led_info['size_x']),
					"size_y"	=> intval($led_info['size_y']),
					"resolu_x"	=> intval($led_info['resolu_x']),
					"resolu_y"	=> intval($led_info['resolu_y'])
				);
				$respones = array("err_code"=>"000000","msg"=>"ok","data"=>$data);
			}else{
				$map = array('id'=>$player['id']);
				$data = array('mac'=>$mac);
				$res = $player_model->where($map)->save($data);
				if($res){
					$led_model = D("Screen");
					$led_info = $led_model->screen_by_id($player['id']);
					$data = array(
						"id"		=> $bind_id,
						"key"		=> $bind_key,
						'mac'		=> $mac,
						'screen_id'	=> $player['id'],
						"name"		=> $led_info['name'],
						"size_x"	=> intval($led_info['size_x']),
						"size_y"	=> intval($led_info['size_y']),
						"resolu_x"	=> intval($led_info['resolu_x']),
						"resolu_y"	=> intval($led_info['resolu_y'])
					);
					$respones = array("err_code"=>"000000","msg"=>"ok","data"=>$data);
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
	 * CmdType	0-发布播放方案
	 * 		  	1-锁定屏幕参数更新
	 * 			2-心跳周期更新
	 * 			3-监控数据上传参数更新
	 * 			4-软件定时开关时间更新
	 * 			5-屏幕参数更新
	 * 			6-屏幕工作时间更新
	 * 			7-播放方案紧急插播
	 * 			8-离线策略
	 * 			9-终端长连接重连
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
			$cmd_model = D("Command");
			$cmds_list = $cmd_model->cmds_list($led_id);
			$cmds = array();
			$cmd_ids = array();
			foreach($cmds_list as $val){
				$cmdParam = array();
				$cmd_ids[] = $val['id'];
				$param = json_decode($val['param'], true);
				switch($val['type']){
					/*0-发布播放方案*/
					case "0":
						$screen_model = D("Screen");
						$plan_model = D("Program");
						$media_model = D("Media");
						$AliyunOSS = new AliyunOSS();
						$plan = $plan_model->program_detail($param['program_id']);
						$screen = $screen_model->screen_by_id($led_id);
						if($plan){
							$medias = array();
							$media_list = json_decode($plan['info'], true);
							foreach($media_list as $v){
								$media = $media_model->media_by_name_md5($v['MediaName'], $v['MediaMD5'], $screen['uid']);
								$media_name_array = explode('.', stripslashes($media['name']));
								$suffix = end($media_name_array);
								$temp_name = array_pop($media_name_array);
								$media_name = implode('.', $media_name_array) . "_{$v['MediaMD5']}.{$suffix}";
								$medias[] = array(
									'MediaId'	=> $media['id'],
									'MediaName'	=> $media_name,
									'MediaUrl'	=> $AliyunOSS->download_uri($this->media_bucket, $media['object'])
								);
							}
							$plan_name_array = explode('.', stripslashes($plan['name']));
							$plan_suffix = end($plan_name_array);
							$temp_name = array_pop($plan_name_array);
							$plan_name = implode('.', $plan_name_array) . "_{$plan['md5']}.{$plan_suffix}";
							$cmdParam = array(
								'ProgramId'		=> $plan['id'],
								'ProgramName'	=> $plan_name,
								'ProgramUrl'	=> $AliyunOSS->download_uri($this->program_bucket, $plan['object']),
								'Medias'		=> $medias
							);
						}
						break;
					/*1-锁定屏幕参数更新*/
					case "1":
						if($param){
							$cmdParam = array(
								"enable" => intval($param['clock']),
								"password" => $param['clock_password']
							);
						}
						break;
					/*2-心跳周期更新*/
					case "2":
						if($param){
							$cmdParam = array(
								"cycle" => intval($param['heartbeat_cycle'])
							);
						}
						break;
					/*3-监控数据上传参数更新*/
					case "3":
						if($param){
							$cmdParam = array(
								"cycle" => intval($param['alarm_cycle']),
								"url" => $param['alarm_url']
							);
						}
						break;
					/*4-软件定时开关时间更新*/
					case "4":
						if($param){
							$cmdParam = array(
								"switch" => intval($param['time_switch']),
								"enable" => $param['soft_enable'],
								"disable" => $param['soft_disable']
							);
						}
						break;
					/*5-屏幕参数更新*/
					case "5":
						if($param){
							$cmdParam = array(
								"id"		=> $id,
								"key"		=> $key,
								"name"		=> $param['name'],
								"size_x"	=> intval($param['size_x']),
								"size_y"	=> intval($param['size_y']),
								"resolu_x"	=> intval($param['resolu_x']),
								"resolu_y"	=> intval($param['resolu_y'])
							);
						}
						break;
					/*6-屏幕工作时间更新*/
					case "6":
						if($param){
							$cmdParam = array(
								"start" => $param['start'],
								"end" => $param['end']
							);
						}
						break;
					/*7-播放方案紧急插播*/
					case "7":
						$screen_model = D("Screen");
						$plan_model = D("Program");
						$media_model = D("Media");
						$AliyunOSS = new AliyunOSS();
						$plan = $plan_model->program_detail($param['program_id']);
						$screen = $screen_model->screen_by_id($led_id);
						if($plan){
							$medias = array();
							$media_list = json_decode($plan['info'], true);
							foreach($media_list as $v){
								$media = $media_model->media_by_name_md5($v['MediaName'], $v['MediaMD5'], $screen['uid']);
								$media_name_array = explode('.', stripslashes($media['name']));
								$suffix = end($media_name_array);
								$temp_name = array_pop($media_name_array);
								$media_name = implode('.', $media_name_array) . "_{$v['MediaMD5']}.{$suffix}";
								$medias[] = array(
									'MediaId'	=> $media['id'],
									'MediaName'	=> $media_name,
									'MediaUrl'	=> $AliyunOSS->download_uri($this->media_bucket, $media['object'])
								);
							}
							$plan_name_array = explode('.', stripslashes($plan['name']));
							$plan_suffix = end($plan_name_array);
							$temp_name = array_pop($plan_name_array);
							$plan_name = implode('.', $plan_name_array) . "_{$plan['md5']}.{$plan_suffix}";
							$cmdParam = array(
								'ProgramId'		=> $plan['id'],
								'ProgramName'	=> $plan_name,
								'ProgramUrl'	=> $AliyunOSS->download_uri($this->program_bucket, $plan['object']),
								'Medias'		=> $medias
							);
						}
						break;
					/*8-离线方案*/
					case "8":
						$screen_model = D("Screen");
						$plan_model = D("Program");
						$media_model = D("Media");
						$AliyunOSS = new AliyunOSS();
						$plan = $plan_model->program_detail($param['program_id']);
						$screen = $screen_model->screen_by_id($led_id);
						if($plan){
							$medias = array();
							$media_list = json_decode($plan['info'], true);
							foreach($media_list as $v){
								$media = $media_model->media_by_name_md5($v['MediaName'], $v['MediaMD5'], $screen['uid']);
								$media_name_array = explode('.', stripslashes($media['name']));
								$suffix = end($media_name_array);
								$temp_name = array_pop($media_name_array);
								$media_name = implode('.', $media_name_array) . "_{$v['MediaMD5']}.{$suffix}";
								$medias[] = array(
									'MediaId'	=> $media['id'],
									'MediaName'	=> $media_name,
									'MediaUrl'	=> $AliyunOSS->download_uri($this->media_bucket, $media['object'])
								);
							}
							$plan_name_array = explode('.', stripslashes($plan['name']));
							$plan_suffix = end($plan_name_array);
							$temp_name = array_pop($plan_name_array);
							$plan_name = implode('.', $plan_name_array) . "_{$plan['md5']}.{$plan_suffix}";
							$cmdParam = array(
								'ProgramId'		=> $plan['id'],
								'ProgramName'	=> $plan_name,
								'ProgramUrl'	=> $AliyunOSS->download_uri($this->program_bucket, $plan['object']),
								'Medias'		=> $medias
							);
						}
						break;
					default:
						break;
				}
				if($cmdParam){
					$cmds[] = array(
						"CmdId"		=>	$val['id'],
						"CmdType"	=>	intval($val['type']),
						"CmdParam"	=>	base64_encode(json_encode($cmdParam))
					);
				}
			}
			/*9-终端长连接重连*/
			//$redis_serv = \Think\Cache::getInstance('Redis', array('host'=>C("redis_server")));
			$redis_serv = new \Redis();
			$redis_serv->connect(C("redis_server"), C("redis_port"));
			$cache_key = md5("{$id}_{$key}_{$mac}_fd");
			$pl_online = $redis_serv->get($cache_key);
			unset($redis_serv);
			if(!$pl_online){
				$cfg_model = D("Config");
				$ws = $cfg_model->ws_config();
				$cmdParam = array(
					/*'ws_ip'	=> $ws['ip'],
					'ws_port' => $ws['port']*/
					'host' => "ws://{$ws['ip']}:{$ws['port']}"
				);
				$cmds[] = array(
					"CmdId"		=>	"0",
					"CmdType"	=>	9,
					"CmdParam"	=>	base64_encode(json_encode($cmdParam))
				);
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
	 * CPU使用率	cpu usage
	 * 硬盘使用率	disk usage
	 * 内存使用率	memory usage
	 * CPU温度	cpu temperature
	 * 风扇转速	fan speed
	 */
	public function monitor(){
		$obj = $this->param;
		$bind_id	= $obj['Id'];
		$bind_key	= $obj['Key'];
		$mac		= strtoupper(str_replace(':', '-', $obj['Mac']));
		$player_model = D("Player");
		$player = $player_model->player_by_bind($bind_id, $bind_key, "id,mac");
		if($mac == $player['mac']){
			$monitor = $obj['Monitor'];
			if($monitor){
				$params = array(
					"Cpu_usage"			=> $monitor['Cpu_usage'],
					"Disk_usage"		=> $monitor['Disk_usage'],
					"Memory_usage"		=> $monitor['Memory_usage'],
					"Cpu_temperature"	=> $monitor['Cpu_temperature'],
					"Fan_speed"			=> $monitor['Fan_speed']
				);
				$screen_id = $player['id'];
				$alarm_model = D("Alarm");
				$alram = $alarm_model->alarm_by_sid($screen_id, 0, "id");
				if($alram){
					$data = array();
					$data['id'] = $alram['id'];
					$data['screen_id'] = $screen_id;
					$data['type'] = 0;
					$data['param'] = json_encode($params);
					$data['up_time'] = NOW_TIME;
					$res = $alarm_model->save($data);
					if($res){
						$respones = array("err_code"=>"000000","msg"=>"ok");
					}else{
						$respones = array("err_code"=>"020302","msg"=>"Monitor data reported failure");
					}
				}else{
					$data = array();
					$data['screen_id'] = $screen_id;
					$data['type'] = 0;
					$data['param'] = json_encode($params);
					$data['up_time'] = NOW_TIME;
					$res = $alarm_model->add($data);
					if($res){
						$respones = array("err_code"=>"000000","msg"=>"ok");
					}else{
						$respones = array("err_code"=>"020302","msg"=>"Monitor data reported failure");
					}
				}
			}else{
				$respones = array("err_code"=>"020301","msg"=>"Monitor data empty");
			}
		}else{
			$respones = array("err_code"=>"020201","msg"=>"Player MAC error");
		}
		$this->ajaxReturn($respones);
	}

	/**
	 * 监控图片上传
	 */
	public function picture(){
		$obj = $this->param;
		$bind_id	= $obj['Id'];
		$bind_key	= $obj['Key'];
		$mac		= strtoupper(str_replace(':', '-', $obj['Mac']));
		$mac		= str_replace('-', '', $obj['Mac']);
		$picture_bucket = C("oss_picture_bucket");
		$AliyunOSS = new AliyunOSS();
		$object = "{$mac}/".date("Ymd")."/".time().".jpg";
		$sign_url = $AliyunOSS->upload_sign_uri($object, $picture_bucket, 1800);
		$respones = array("err_code"=>"000000","msg"=>"ok","url"=>$sign_url);
		$this->ajaxReturn($respones);
	}
	
	/**
	 * 命令执行结果
	 */
	public function cmd_result(){
		$obj = $this->param;
		$bind_id	= $obj['Id'];
		$bind_key	= $obj['Key'];
		$mac		= strtoupper(str_replace(':', '-', $obj['Mac']));
		$player_model = D("Player");
		$player = $player_model->player_by_bind($bind_id, $bind_key, "id,mac");
		if($mac == $player['mac']){
			$cmd_id = $obj['CmdId'];
			$cmd_res = $obj['CmdRes'];
			if($cmd_id){
				$cmd_model = D("Command");
				$map = array("id" => $cmd_id);
				$status = $cmd_res ? 2 : 3;
				$res = $cmd_model->where($map)->setField("status", $status);
				if($res !== false){
					$respones = array("err_code"=>"000000","msg"=>"ok");
				}else{
					$respones = array("err_code"=>"020402","msg"=>"failure");
				}
			}else{
				$respones = array("err_code"=>"020401","msg"=>"Command id error");
			}
		}else{
			$respones = array("err_code"=>"020201","msg"=>"Player MAC error");
		}
		$this->ajaxReturn($respones);
	}
	
	/**
	 * 上传播放记录
	 */
	public function record(){
		$obj = $this->param;
		$bind_id	= $obj['Id'];
		$bind_key	= $obj['Key'];
		$mac		= strtoupper(str_replace(':', '-', $obj['Mac']));
		$player_model = D("Player");
		$player = $player_model->player_by_bind($bind_id, $bind_key, "id,mac");
		if($mac == $player['mac']){
			$media_model = D("Media");
			$led_model = D("Screen");
			$media_name = end(explode('/', str_replace('\\', '/', $obj['MediaName'])));
			$media_md5 = strtolower($obj['MediaMD5']);
			$led_info = $led_model->screen_by_id($player['id']);
			$user_id = $led_info['id'];
			$media_id = $media_model->media_exists($media_name, $media_md5, $user_id);
			if($media_id){
				$data = array();
				$data['screen_id'] = $player['id'];
				$data['media_name'] = mysql_escape_string($media_name);
				$data['media_md5'] = $media_md5;
				$data['start'] = $obj['StartTime'];
				$data['end'] = $obj['EndTime'];
				$data['addtime'] = NOW_TIME;
				$record_model = D("Record");
				$res = $record_model->add($data);
				if($res){
					$respones = array("err_code"=>"000000","msg"=>"ok");
				}else{
					$respones = array("err_code"=>"020503","msg"=>"Report record failed");
				}
			}else{
				$respones = array("err_code"=>"020501","msg"=>"Media not found");
			}
		}else{
			$respones = array("err_code"=>"020201","msg"=>"Player MAC error");
		}
		$this->ajaxReturn($respones);
	}

	public function screen(){
		$obj = $this->param;
		$bind_id	= $obj['Id'];
		$bind_key	= $obj['Key'];
		$mac		= strtoupper(str_replace(':', '-', $obj['Mac']));
		$player_model = D("Player");
		$player = $player_model->player_by_bind($bind_id, $bind_key, "id,mac");
		if($mac == $player['mac']){
			$screen_model = D("Screen");
			$screen = $screen_model->screen_by_id($player['id']);
			$regions = D("Region")->all_region_name();
			$province = $regions[$screen['province']];
			$city = $regions[$screen['city']];
			$district = $regions[$screen['district']];
			$respones = array(
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
		}else{
			$respones = array("err_code"=>"020201","msg"=>"Player MAC error");
		}
		$this->ajaxReturn($respones);
	}
}
