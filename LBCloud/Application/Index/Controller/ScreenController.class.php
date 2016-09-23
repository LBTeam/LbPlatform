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
}
