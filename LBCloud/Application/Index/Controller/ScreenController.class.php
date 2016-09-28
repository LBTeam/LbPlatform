<?php
/**
 * 屏幕管理controller
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Controller;

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
    		"type" => array(
    			0 => "室外",
    			1 => "室内"
    		),
    		"operate" => array(
    			0 => "全包",
    			1 => "分时"
    		),
    		"province" => $regions,
    		"city" => $regions,
    		"district" => $regions
    	);
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
}
