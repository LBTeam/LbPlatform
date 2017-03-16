<?php
/**
 * 屏幕管理controller
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Controller;
use Api\Service\AliyunOSS;

class ScreenController extends CommonController
{
	/**
	 * 列表
	 */
	public function index(){
		$led_model = D("Screen");
		$region_model = D("Region");
		$leds = $led_model->screen_by_uid(ADMIN_UID);
		$regions = $region_model->all_region();
		$condition = array(
    		"type" => array(0=>"室外",1=>"室内"),
    		"operate" => array(0=>"全包",1=>"分时"),
    		"online" => array(0=>"离线",1=>"在线",2=>"未绑定"),
    		"province" => $regions,
    		"city" => $regions,
    		"district" => $regions
    	);
		try{
			$redis_serv = \Think\Cache::getInstance('Redis', array('host'=>C("redis_server")));
			foreach($leds as &$val){
				$pl_mac = strtoupper(str_replace(':', '-', $val['mac']));
				if($pl_mac){
					$pl_id = $val["bind_id"];
					$pl_key = $val["bind_key"];
					$cache_key = md5("{$pl_id}_{$pl_key}_{$pl_mac}_fd");
					$pl_online = $redis_serv->get($cache_key);
					//$pl_online = true;
					$val['online'] = $pl_online ? 1 : 0;
				}else{
					$val['online'] = 2;
				}
			}
		}catch(\Exception $e){
			foreach($leds as &$val){
				$val['online'] = 0;
			}
		}
		int_to_string($leds, $condition);
    	$user_model = D("User");
		if($user_model->is_normal(ADMIN_UID)){
			$this->assign("is_normal", 1);
		}else{
			$this->assign("is_normal", 0);
		}
		$this->assign("leds", $leds);
		$this->meta_title = "屏幕列表";
		$this->display();
	}
	
	/**
	 * 添加
	 */
	public function add(){
		if(IS_POST){
			$led_model = D("Screen");
			$user_model = D("User");
			$rules = array(
				array('name','require','屏幕名称不能为空！'),
				array('name','','屏幕名称已经存在！',0,'unique',1),
				array('size_x','require','屏幕尺寸X不能为空！'),
				array('size_y','require','屏幕尺寸Y不能为空！'),
				array('resolu_x','require','分辨率X不能为空！'),
				array('resolu_y','require','分辨率Y不能为空！'),
				array('longitude','require','经度不能为空！'),
				array('latitude','require','纬度不能为空！')
			);
			if(!$user_model->is_normal(ADMIN_UID)){
				$rules[] = array('uid','require','请选择拥有者！');
			}
			$rules[] = array('province','require','请选择省！');
			$rules[] = array('city','require','请选择市！');
			$rules[] = array('address','require','街道不能为空！');
			if($led_model->validate($rules)->create()){
				$led_data = array();
				$led_data['name'] = I("post.name");
				$led_data['remark'] = I("post.remark");
				$led_data['size_x'] = I("post.size_x");
				$led_data['size_y'] = I("post.size_y");
				$led_data['resolu_x'] = I("post.resolu_x");
				$led_data['resolu_y'] = I("post.resolu_y");
				$led_data['type'] = I("post.type", 0);
				$led_data['operate'] = I("post.operate", 0);
				$led_data['longitude'] = I("post.longitude");
				$led_data['latitude'] = I("post.latitude");
				$led_data['uid'] = I("post.uid") ? I("post.uid") : ADMIN_UID;
				$led_data['province'] = I("post.province");
				$led_data['city'] = I("post.city");
				$led_data['district'] = I("post.district", 0);
				$led_data['address'] = I("post.address");
				$led_data['file'] = I("post.file", "");
				$led_data['db_version'] = I("post.db_version", "");
				$led_data['addtime'] = NOW_TIME;
				$player_model = D("Player");
				$model = new \Think\Model();
				$model->startTrans();
				$led_id = $led_model->add($led_data);
				$group_id = I("post.group_id", 0);
				$player_data = array();
				$player_data['id'] = $led_id;
				$player_data['bind_id'] = $player_model->bind_id();
				$player_data['bind_key'] = $player_model->bind_key();
				$player_res = $player_model->add($player_data);
				$bind_res = true;
				if($group_id){
					$bind_res = $led_model->bind_group(ADMIN_UID, $led_id, $group_id);
				}
				if($led_id && $player_res && $bind_res){
					$model->commit();
					/*命令下发开始*/
					/*$param = array(
						"name"		=> $led_data['name'],
						"size_x"	=> $led_data['size_x'],
						"size_y"	=> $led_data['size_y'],
						"resolu_x"	=> $led_data['resolu_x'],
						"resolu_y"	=> $led_data['resolu_y']
					);
					$cmd_data = array();
					$cmd_data['screen_id'] = $led_id;
					$cmd_data['type'] = 5;
					$cmd_data['param'] = json_encode($param);
					$cmd_data['publish'] = NOW_TIME;
					$cmd_data['execute'] = NOW_TIME;
					$cmd_data['expired'] = NOW_TIME;
					$cmd_model = D("Command");
					$cmd_model->add($cmd_data);*/
					/*命令下发结束*/
					$this->success('新增成功', U('index'));
				}else{
					$model->rollback();
					$this->error('新增失败');
				}
			}else{
				$this->error($led_model->getError());
			}
		}else{
			$user_model = D("User");
			$group_model = D("Group");
			$region_model = D("Region");
			if($user_model->is_normal(ADMIN_UID)){
				$this->assign("is_normal", 1);
			}else{
				$owner = $user_model->users_by_puid(ADMIN_UID);
				$this->assign("owner", $owner);
				$this->assign("is_normal", 0);
			}
			$groups = $group_model->group_by_uid(ADMIN_UID);
			$provinces = $region_model->all_region(0);
			$this->assign("groups", $groups);
			$this->assign("provinces", $provinces);
			$this->meta_title = "添加屏幕";
			$this->display();
		}
	}
	
	/**
	 * 修改
	 */
	public function edit($id=0){
		if(IS_POST){
			$led_model = D("Screen");
			$user_model = D("User");
			$rules = array(
				array('id','require','屏幕Id不能为空！'),
				array('name','require','屏幕名称不能为空！'),
				array('name','','屏幕名称已经存在！',0,'unique',2),
				array('size_x','require','屏幕尺寸X不能为空！'),
				array('size_y','require','屏幕尺寸Y不能为空！'),
				array('resolu_x','require','分辨率X不能为空！'),
				array('resolu_y','require','分辨率Y不能为空！'),
				array('longitude','require','经度不能为空！'),
				array('latitude','require','纬度不能为空！')
			);
			if(!$user_model->is_normal(ADMIN_UID)){
				$rules[] = array('uid','require','请选择拥有者！');
			}
			$rules[] = array('province','require','请选择省！');
			$rules[] = array('city','require','请选择市！');
			$rules[] = array('address','require','街道不能为空！');
			if($led_model->validate($rules)->create()){
				$led_data = array();
				$led_data['id'] = I("post.id");
				$led_data['name'] = I("post.name");
				$led_data['remark'] = I("post.remark");
				$led_data['size_x'] = I("post.size_x");
				$led_data['size_y'] = I("post.size_y");
				$led_data['resolu_x'] = I("post.resolu_x");
				$led_data['resolu_y'] = I("post.resolu_y");
				$led_data['type'] = I("post.type", 0);
				$led_data['operate'] = I("post.operate", 0);
				$led_data['longitude'] = I("post.longitude");
				$led_data['latitude'] = I("post.latitude");
				$led_data['uid'] = I("post.uid") ? I("post.uid") : ADMIN_UID;
				$led_data['province'] = I("post.province");
				$led_data['city'] = I("post.city");
				$led_data['district'] = I("post.district", 0);
				$led_data['address'] = I("post.address");
				$led_data['file'] = I("post.file", "");
				$led_data['db_version'] = I("post.db_version", "");
				$group_id = I("post.group_id", 0);
				$model = new \Think\Model();
				$model->startTrans();
				$save_res = $led_model->save($led_data);
				$unbind_res = $led_model->unbind_group(ADMIN_UID, $led_data['id']);
				$bind_res = true;
				if($group_id){
					$bind_res = $led_model->bind_group(ADMIN_UID, $led_data['id'], $group_id);
				}
				if($save_res !== false && $unbind_res !== false && $bind_res){
					$model->commit();
					if($save_res){
						/*命令下发开始*/
						$param = array(
							"name"		=> $led_data['name'],
							"size_x"	=> $led_data['size_x'],
							"size_y"	=> $led_data['size_y'],
							"resolu_x"	=> $led_data['resolu_x'],
							"resolu_y"	=> $led_data['resolu_y']
						);
						$cmd_data = array();
						$cmd_data['screen_id'] = $id;
						$cmd_data['type'] = 5;
						$cmd_data['param'] = json_encode($param);
						$cmd_data['publish'] = NOW_TIME;
						$cmd_data['execute'] = NOW_TIME;
						$cmd_data['expired'] = NOW_TIME;
						$cmd_model = D("Command");
						$cmd_model->rm_cmd_by_sid($id, 5);
						$cmd_model->add($cmd_data);
						/*命令下发结束*/
					}
					$this->success('修改成功', U('index'));
				}else{
					$model->rollback();
					$this->error('修改失败');
				}
			}else{
				$this->error($led_model->getError());
			}
		}else{
			$led_model = D("Screen");
			$info = $led_model->screen_by_id($id);
			if($info){
				$user_model = D("User");
				$group_model = D("Group");
				$region_model = D("Region");
				if($user_model->is_normal(ADMIN_UID)){
					$this->assign("is_normal", 1);
				}else{
					$owner = $user_model->users_by_puid(ADMIN_UID);
					$this->assign("owner", $owner);
					$this->assign("is_normal", 0);
				}
				$groups = $group_model->group_by_uid(ADMIN_UID);
				$provinces = $region_model->all_region(0);
				$citys = $region_model->all_region($info['province']);
				$districts = $region_model->all_region($info['city']);
				$this->assign("groups", $groups);
				$this->assign("provinces", $provinces);
				$this->assign("citys", $citys);
				$this->assign("districts", $districts);
				$this->assign('info', $info);
	            $this->meta_title = '编辑屏幕';
	            $this->display();
			}else{
				$this->error('获取屏幕信息错误');
			}
		}
	}

	/**
	 * 查看
	 */
	public function show($id=0){
		$led_model = D("Screen");
		$info = $led_model->screen_by_id($id);
		if($info){
			$user_model = D("User");
			$group_model = D("Group");
			$region_model = D("Region");
			if($user_model->is_normal(ADMIN_UID)){
				$this->assign("is_normal", 1);
			}else{
				$owner = $user_model->users_by_puid(ADMIN_UID);
				$this->assign("owner", $owner);
				$this->assign("is_normal", 0);
			}
			$groups = $group_model->group_by_uid(ADMIN_UID);
			$provinces = $region_model->all_region(0);
			$citys = $region_model->all_region($info['province']);
			$districts = $region_model->all_region($info['city']);
			$this->assign("groups", $groups);
			$this->assign("provinces", $provinces);
			$this->assign("citys", $citys);
			$this->assign("districts", $districts);
			$this->assign('info', $info);
            $this->meta_title = '查看屏幕';
            $this->display();
		}else{
			$this->error('获取屏幕信息错误');
		}
	}
	
	/**
	 * 删除
	 */
	public function del(){
		$id = I('request.id', 0);
        if ( empty($id) ) {
            $this->error('请选择要操作的数据!');
        }
		$led_model = D('Screen');
		$id = array_unique((array)$id);
        $map = array('id' => array('in', $id) );
        if($led_model->where($map)->setField("is_delete", 1)){
        	$led_model->unbind_group(0, $id);
            $this->success('删除成功');
        } else {
            $this->error('删除失败！');
        }
	}
	
	/*监控数据*/
	public function monitor($id=0){
		$alarm_model = D("Alarm");
		$alarms = $alarm_model->alarm_by_sid($id);
		if($alarms){
			$led_model = D("Screen");
			$player_model = D("Player");
			$led_info = $led_model->screen_by_id($id, "s.name");
			$player = $player_model->player_by_id($id);
			$monitor = json_decode($alarms['param'], true);
			$monitor['addtime'] = $alarms['up_time'];
			$this->meta_title = "监控数据";
			$this->assign('monitor', $monitor);
			$this->assign('name', $led_info['name']);
			$this->assign('p_name', $player['name']);
			$this->display();
		}else{
			$this->error('播放器暂无监控数据！');
		}
	}
	
	/*告警配置*/
	public function alarm($id=0){
		if(IS_POST){
			$alarm_set_m = D("AlarmSet");
			$rules = array(
				array('screen_id','require','系统错误，非法访问！'),
				array('cpu_usage','require','CPU使用率不能为空！'),
				array('cpu_usage',array(1,100),'CPU使用率在1-100之间，单位%！', 0, 'between', 3),
				array('disk_usage','require','硬盘使用率不能为空！'),
				array('disk_usage',array(1,100),'硬盘使用率在1-100之间，单位%！', 0, 'between', 3),
				array('memory_usage','require','内存使用率不能为空！'),
				array('memory_usage',array(1,100),'内存使用率在1-100之间，单位%！', 0, 'between', 3),
				array('cpu_temperature','require','CPU温度不能为空！'),
				array('cpu_temperature','/^[1-9][0-9]*$/','CPU温度必须为大于0的整数！'),
				array('fan_speed','require','风扇转速不能为空！'),
				array('fan_speed','/^[1-9][0-9]*$/','风扇转速必须为大于0的整数！'),
				array('is_auto','require','系统错误，非法访问！'),
				array('is_auto',array(0,1),'系统错误，非法访问！',1,'in'),
				array('alarm_mode','require','系统错误，非法访问！'),
				array('alarm_mode',array(0,1),'系统错误，非法访问！',1,'in'),
			);
			if($alarm_set_m->validate($rules)->create()){
				$data = array();
				$data['screen_id'] = I("post.screen_id");
				$data['cpu_usage'] = I("post.cpu_usage");
				$data['disk_usage'] = I("post.disk_usage");
				$data['memory_usage'] = I("post.memory_usage");
				$data['cpu_temperature'] = I("post.cpu_temperature");
				$data['fan_speed'] = I("post.fan_speed");
				$data['is_auto'] = I("post.is_auto");
				$data['alarm_mode'] = I("post.alarm_mode");
				$res = $alarm_set_m->update_set($data);
				if($res !== false){
					$this->success("告警配置设置成功！");
				}else{
					$this->error("告警配置设置失败！");
				}
			}else{
				$this->error($alarm_set_m->getError());
			}
		}else{
			$led_model = D("Screen");
			$info = $led_model->screen_by_id($id);
			if($info){
				$player_model = D("Player");
				$set_model = D("AlarmSet");
				$player = $player_model->player_by_id($id);
				$sets = $set_model->set_by_sid($id);
				$this->meta_title = "告警配置";
				$this->assign('name', $info['name']);
				$this->assign('p_name', $player['name']);
				$this->assign('sets', $sets);
				$this->display();
			}else{
				$this->error('屏幕不存在！');
			}
		}
	}

	/**
	 * 监控图片
	 */
	public function picture($id=0){
		$player_model = D("Player");
		$player = $player_model->player_by_id($id);
		if($player && $player['mac']){
			$led_model = D("Screen");
			if($led_model->check_screen_operation($id)){
				$mac = str_replace('-', '', $player['mac']);
				//$mac = "D07E355210CB";
				$date = date('Ymd');
				$AliyunOSS = new AliyunOSS();
				$picture_bucket = C("oss_picture_bucket");
				$prefix = "{$mac}/{$date}/";
				$object = '';
				$marker = false;
				while(true){
					$o_list = $AliyunOSS->object_list($picture_bucket, $prefix, 1000, $marker);
					if($o_list['objects']){
						if(count($o_list['objects']) == 1000){
							$temp_marker = end($o_list['objects']);
							$marker = $temp_marker['key'];
							continue;
						}else{
							$end_obj = end($o_list['objects']);
							$object = $end_obj['key'];
							break;
						}
					}else{
						break;
					}
				}
				if($object){
					$uri = $AliyunOSS->download_uri($picture_bucket, $object);
					$this->assign("uri", $uri);
					$this->display();
				}else{
					$this->error("播放器暂无监控图片！", "about:blank");
				}
			}else{
				$this->error("系统错误：权限拒绝！", "about:blank");
			}
		}else{
			$this->error("系统错误：非法操作！", "about:blank");
		}
	}
	
	/**
	 * 播放器
	 */
	public function player($id = 0){
		if(IS_POST){
			$player_model = D("Player");
			$rules = array(
				array('id','require','屏幕Id不能为空！'),
				array('name','require','播放器名称不能为空！'),
				array('mode','require','播放器模式不能为空！'),
				array('start','require','请选择工作开始时间！'),
				array('end','require','请选择工作结束时间！'),
				array('start','check_work_time','开始时间不得大于结束时间！',1,'function')
			);
			if($player_model->validate($rules)->create()){
				$map = array();
				$map['id'] = I("post.id", 0);
				$data = array();
				$data['name'] = I("post.name", "");
				$data['remark'] = I("post.remark", "");
				$data['mode'] = I("post.mode", "");
				$data['start'] = I("post.start");
				$data['end'] = I("post.end");
				$player_res = $player_model->where($map)->save($data);
				if($player_res !== false){
					if($player_res){
						/*命令下发开始*/
						$param = array(
							"start"=>$data['start'],
							"end"=>$data['end']
						);
						$cmd_data = array();
						$cmd_data['screen_id'] = $id;
						$cmd_data['type'] = 6;
						$cmd_data['param'] = json_encode($param);
						$cmd_data['publish'] = NOW_TIME;
						$cmd_data['execute'] = NOW_TIME;
						$cmd_data['expired'] = NOW_TIME;
						$cmd_model = D("Command");
						$cmd_model->rm_cmd_by_sid($id, 6);
						$cmd_model->add($cmd_data);
						/*命令下发结束*/
					}
					$this->success('操作成功', U('index'));
				}else{
					$this->error('操作失败');
				}
			}else{
				$this->error($player_model->getError());
			}
		}else{
			$player_model = D("Player");
			$info = $player_model->player_by_id($id);
			if($info !== false){
				$this->assign('info', $info);
				$this->meta_title = '修改播放器';
            	$this->display();
			}else{
				$this->error('获取播放器信息错误');
			}
		}
	}
	
	/**
	 * 时段价格
	 */
	public function price($id=0){
		$price_model = D("Price");
		$prices = $price_model->price_by_screen($id);
		if($prices !== false){
			$led_model = D("Screen");
			$info = $led_model->screen_by_id($id, 's.name');
			$this->assign('prices', $prices);
			$this->assign('l_id', $id);
			$this->assign('l_name', $info['name']);
			$this->meta_title = '时段价格列表';
        	$this->display();
		}else{
			$this->error('获取屏幕价格失败');
		}
	}
	
	/**
	 * 添加时段价格
	 */
	public function add_price($sid=0){
		if(IS_POST){
			$price_model = D("Price");
			$rules = array(
				array('screen_id','require','屏幕Id不能为空！'),
				array('start','require','请选择开始时间！'),
				array('end','require','请选择结束时间！'),
				array('start','check_work_time','开始时间不得大于结束时间！',1,'function'),
				array('start','check_price_cross','与现有时间段重复！',1,'function'),
				array('price','require','价格不能为空！'),
				array('price','currency','价格格式错误！'),
			);
			if($price_model->validate($rules)->create()){
				$data = array();
				$data['screen_id'] = I("post.screen_id");
				$data['start'] = I("post.start");
				$data['end'] = I("post.end");
				$data['price'] = I("post.price");
				$price_id = $price_model->add($data);
				if($price_id){
					$this->success('添加成功', U('price?id='.$data['screen_id']));
				}else{
					$this->error('添加失败');
				}
			}else{
				$this->error($price_model->getError());
			}
		}else{
			if($sid){
				$this->assign("sid", $sid);
				$this->meta_title = "添加时段价格";
				$this->display();
			}else{
				$this->error("添加失败，系统错误！");
			}
		}
	}
	
	/**
	 * 修改时段价格
	 */
	public function edit_price($id=0){
		if(IS_POST){
			$price_model = D("Price");
			$rules = array(
				array('id','require','价格Id不能为空！'),
				array('start','require','请选择开始时间！'),
				array('end','require','请选择结束时间！'),
				array('start','check_work_time','开始时间不得大于结束时间！',1,'function'),
				array('start','check_price_cross','与现有时间段重复！',1,'function'),
				array('price','require','价格不能为空！'),
				array('price','currency','价格格式错误！'),
			);
			if($price_model->validate($rules)->create()){
				$data = array();
				$data['id'] = I("post.id");
				$data['start'] = I("post.start");
				$data['end'] = I("post.end");
				$data['price'] = I("post.price");
				$save_res = $price_model->save($data);
				if($save_res !== false){
					$this->success('修改成功', U('price?id='.I("post.screen_id")));
				}else{
					$this->error('修改失败');
				}
			}else{
				$this->error($price_model->getError());
			}
		}else{
			$price_model = D("Price");
			$info = $price_model->price_by_id($id);
			if($info){
				$this->assign('info', $info);
				$this->meta_title = "修改时段价格";
				$this->display();
			}else{
				$this->error("获取价格信息错误！");
			}
		}
	}
	
	/**
	 * 删除时段价格
	 */
	public function del_price(){
		$id = I('request.id', 0);
        if ( empty($id) ) {
            $this->error('请选择要操作的数据!');
        }
		$price_model = D("Price");
		$id = array_unique((array)$id);
        $map = array('id' => array('in', $id) );
        if($price_model->where($map)->delete()){
            $this->success('删除成功');
        } else {
            $this->error('删除失败！');
        }
	}
	
	/**
	 * 播放器参数配置
	 * @param $item 配置项目
	 * 			1-设置锁定屏幕密码
	 * 			2-设置心跳周期
	 * 			3-设置监控数据上传周期
	 * 			4-设置定时开关
	 */
	public function setting($id=0,$item=1){
		if(IS_POST){
			if($id){
				switch($item){
					case 1:
						$rules = array();
						$rules[] = array('clock','require','请选择锁定开关！');
						$rules[] = array('clock_password','require','锁定密码不能为空！');
						$rules[] = array('clock_password', "/^[A-Za-z0-9]{6,16}$/", '锁定密码格式错误，6-16位字母数字组合！');
						break;
					case 2:
						$rules = array();
						$rules[] = array('heartbeat_cycle','require','心跳周期不能为空！');
						$rules[] = array('heartbeat_cycle','number','心跳周期为数字！');
						break;
					case 3:
						$rules = array();
						$rules[] = array('alarm_cycle','require','上传周期不能为空！');
						$rules[] = array('alarm_cycle','number','上传周期为数字！');
						$rules[] = array('alarm_url','require','上传路径不能为空！');
						break;
					case 4:
						$rules = array();
						$rules[] = array('time_switch','require','请选择定时开关！');
						$rules[] = array('soft_enable','require','开启软件时间不能为空！');
						$rules[] = array('soft_disable','require','关闭软件时间不能为空！');
						break;
					default:
						$item = 1;
						$rules = array();
						$rules[] = array('clock','require','请选择锁定开关！');
						$rules[] = array('clock_password','require','锁定密码不能为空！');
						break;
				}
				$set_model = D("Setting");
				if($set_model->validate($rules)->create()){
					$data = array();
					if(isset($_POST['clock'])){
						$data['clock'] = I("post.clock");
					}
					if(isset($_POST['clock_password'])){
						$data['clock_password'] = I("post.clock_password");
					}
					if(isset($_POST['heartbeat_cycle'])){
						$data['heartbeat_cycle'] = I("post.heartbeat_cycle");
					}
					if(isset($_POST['alarm_cycle'])){
						$data['alarm_cycle'] = I("post.alarm_cycle");
					}
					if(isset($_POST['alarm_url'])){
						$data['alarm_url'] = I("post.alarm_url");
					}
					if(isset($_POST['time_switch'])){
						$data['time_switch'] = I("post.time_switch");
					}
					if(isset($_POST['soft_enable'])){
						$data['soft_enable'] = I("post.soft_enable");
					}
					if(isset($_POST['soft_disable'])){
						$data['soft_disable'] = I("post.soft_disable");
					}
					$settings = $set_model->set_by_sid($id, "id");
					if($settings){
						$map = array("id"=>$id);
						$res = $set_model->where($map)->save($data);
						if($res !== false){
							if($res){
								/*命令下发开始*/
								$cmd_data = array();
								$cmd_data['screen_id'] = $id;
								$cmd_data['type'] = $item;
								$cmd_data['param'] = json_encode($data);
								$cmd_data['publish'] = NOW_TIME;
								$cmd_data['execute'] = NOW_TIME;
								$cmd_data['expired'] = NOW_TIME;
								$cmd_model = D("Command");
								$cmd_model->rm_cmd_by_sid($id, $item);
								$cmd_model->add($cmd_data);
								/*命令下发结束*/
							}
							$this->success("修改成功！");
						}else{
							$this->error("修改失败！");
						}
					}else{
						$data['id'] = $id;
						$res = $set_model->add($data);
						if($res){
							/*命令下发开始*/
							$cmd_data = array();
							$cmd_data['screen_id'] = $id;
							$cmd_data['type'] = $item;
							$cmd_data['param'] = json_encode($data);
							$cmd_data['publish'] = NOW_TIME;
							$cmd_data['execute'] = NOW_TIME;
							$cmd_data['expired'] = NOW_TIME;
							$cmd_model = D("Command");
							$cmd_model->rm_cmd_by_sid($id, $item);
							$cmd_model->add($cmd_data);
							/*命令下发结束*/
							$this->success("修改成功！");
						}else{
							$this->error("修改失败！");
						}
					}
				}else{
					$this->error($set_model->getError());
				}
			}else{
				$this->error("系统错误，非法访问！");
			}
		}else{
			if($id){
				$set_model = D("Setting");
				$settings = $set_model->set_by_sid($id);
				$template = "setting_{$item}";
				$this->assign("screen_id", $id);
				$this->assign("info", $settings);
				$this->meta_title = "播放器参数配置";
				$this->display($template);
			}else{
				$this->error("系统错误，非法访问！");
			}
		}
	}

	/**
	 * 一键关屏
	 */
	public function shutdown(){
		$id = I('request.id', 0);
        if ( empty($id) ) {
            $this->error('请选择要操作的数据!');
        }
		$player_model = D("Player");
		$player = $player_model->player_by_id($id);
		if($player && $player['mac']){
			if(IS_POST){
				$password = I("post.password", "");
				if($password){
					$user_model = D("User");
					$user_info = $user_model->user_by_id(ADMIN_UID);
					$db_pass = $user_info['password'];
					if(sp_compare_password($password, $db_pass)){
						$ws_in = array();
						$ws_in['Act'] = "shutdown";
						$ws_in['Id'] = $player['bind_id'];
						$ws_in['Key'] = $player['bind_key'];
						$ws_in['Mac'] = strtoupper(str_replace(':', '-', $player['mac']));
						$cfg_model = D("Config");
						$ws = $cfg_model->websocket();
						$ws_ip = $ws['ip'];
						$ws_port = $ws['port'];
						import("@.Service.WebsocketClient", '', ".php");
						$client = new \WebSocketClient();
						$client->connect($ws_ip, $ws_port, '/');
						$resp = $client->sendData(json_encode($ws_in));
						$client->disconnect();
						unset($client);
						if( $resp !== true ){
							$this->error("发送失败！");
						}else{
							$this->success("屏幕关闭成功！", U('index'));
						}
					}else{
						$this->error("用户登录密码错误！");
					}
				}else{
					$this->error("用户登录密码不能为空！");
				}
			}else{
				$led_model = D("Screen");
				$led_info = $led_model->screen_by_id($id, "s.name");
				$this->meta_title = "关闭屏幕";
				$this->assign("id", $id);
				$this->assign("name", $led_info['name']);
				$this->display();
			}
		}else{
			$this->error("屏幕未绑定播放器！");
		}
	}
	
	/**
	 * 紧急通知
	 */
	public function notify($id=0){
        if ( empty($id) ) {
            $this->error('请选择要操作的数据!');
        }
		if(IS_POST){
			$content = I("post.content", "");
			if($content){
				$player_model = D("Player");
				$player = $player_model->player_by_id($id);
				if($player && $player['mac']){
					$ws_in = array();
					$ws_in['Act'] = "notice";
					$ws_in['Id'] = $player['bind_id'];
					$ws_in['Key'] = $player['bind_key'];
					$ws_in['Mac'] = strtoupper(str_replace(':', '-', $player['mac']));
					$ws_in['Content'] = trim($content);
					$cfg_model = D("Config");
					$ws = $cfg_model->websocket();
					import("@.Service.WebsocketClient", '', ".php");
					$client = new \WebSocketClient();
					$client->connect($ws['ip'], $ws['port'], '/');
					$resp = $client->sendData(json_encode($ws_in));
					unset($client);
					if( $resp !== true ){
						$this->error("紧急通知发步失败！");
					}else{
						$this->success("紧急通知发步成功！");
					}
				}else{
					$this->error("屏幕未绑定播放器！");
				}
			}else{
				$this->error("通知内容不能为空！");
			}
		}else{
			$this->assign("id", $id);
			$this->meta_title = "发布紧急通知";
			$this->display();
		}
	}

    /**
     * 播放端版本信息
     */
	public function version($id=0){
        $player = D("Player")->player_by_id($id);
        if($player){
            $now = $player['version'];
            $last = D("Version")->last_package();
            dump($now);
            dump($last);
        }else{
            $this->error('屏幕不存在！');
        }
    }

    /**
     * 播放端更新命令发布
     */
    public function ajax_do_upgrade(){
        if(IS_POST){
            $screen_id = I("post.screen_id", 0);
            if(empty($screen_id)){
                $this->error('系统错误，非法操作!');
            }else{
                if(D("Screen")->check_screen_operation($screen_id)){
                    $player = D("Player")->player_by_id($screen_id);
                    $now = $player['version'];
                    $last = D("Version")->last_package();
                    if($now == $last['version']){
                        $this->error("播放器为最高版本，无需更新！");
                    }else{
                        $param = array(
                            "version"   => $last['version'],
                            "url"       => $last['url']
                        );
                        $cmd = array();
                        $cmd['screen_id'] = $screen_id;
                        $cmd['type'] = 10;
                        $cmd['param'] = json_encode($param);
                        $cmd['publish'] = NOW_TIME;
                        $cmd['execute'] = NOW_TIME;
                        $cmd['expired'] = NOW_TIME;
                        $m = new \Think\Model();
                        $m->startTrans();
                        $model = D("Command");
                        $_d = $model->rm_cmd_by_sid($screen_id, 10);
                        $_a = $model->add($cmd);
                        if($_d !== false && $_a){
                            $m->commit();
                            $this->success('更新命名发布成功！');
                        }else{
                            $model->rollback();
                            $this->error('更新命名发布失败！');
                        }
                    }
                }else{
                    $this->error("系统错误：权限拒绝！");
                }
            }
        }else{
            $this->error('系统错误，非法操作！');
        }
    }
}
