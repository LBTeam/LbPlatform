<?php
/**
 * 屏幕组model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Api\Model;
use Think\Model;

class GroupModel extends Model
{
	/**
	 * 用户屏幕组
	 */
	public function user_all_group($user_id){
		return $this->where("uid = {$user_id}")->select();
	}
	
	/**
	 * 分组包含的所有屏幕Id
	 * @param $groups 分组
	 * @param $user_id 用户ID
	 * @return array
	 */
	public function group_screens($groups, $user_id){
		$map = array();
		$map['uid'] = $user_id;
		$map['group_id'] = array('IN', $groups);
		$screens = $this
				->table(C("DB_PREFIX")."group_screen")
				->where($map)
				->getField("screen_id", true);
		if($screens){
			return array_unique($screens);
		}else{
			return array();
		}
	}
}
