<?php
/**
 * 屏幕分组model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Model;
use Think\Model;

class GroupModel extends Model
{
	/**
	 * 屏幕组列表
	 */
	public function group_by_uid($uid){
		return $this->where("uid={$uid}")->select();
	}
	
	/**
	 * 屏幕组详情
	 */
	public function group_by_id($id, $field="*"){
		if($id){
			return $this->field($field)->find($id);
		}
		return array();
	}
	
	/**
	 * 屏幕组所有者
	 */
	public function group_owner($id){
		return $this->where("id={$id}")->getField("uid");
	}
	
	/**
	 * 解绑屏幕及屏幕组关系
	 */
	public function unbind_relation($group_id=0, $screen_id=0){
		$map = array();
		if($group_id){
			$map['group_id'] = array('IN', $group_id);
		}
		if($screen_id){
			$map['screen_id'] = array('IN', $screen_id);
		}
		return $this->table("player_group_screen")->where($map)->delete();
	}
	
	/**
	 * 检查屏幕组是否存在
	 */
	public function check_group($name, $uid, $id=0){
		$map = array();
		$map['uid'] = $uid;
		$map['name'] = $name;
		if($id){
			$map['id'] = array('NEQ', $id);
		}
		$exist = $this->where($map)->getField("id");
		if($exist){
			return true;
		}else{
			return false;
		}
	}
	
	/**
	 * 屏幕列表
	 */
	public function screens_by_gid($group_id){
		if($group_id){
			return $this
					->table("player_group_screen")
					->alias("g")
					->field("g.group_id,s.*,p.bind_id,p.bind_key,u.email AS u_email,u.phone AS u_phone")
					->join("player_screen AS s ON s.id = g.screen_id")
					->join("player_user AS u ON u.uid = s.uid")
					->join("player_player AS p ON p.id = s.id")
					->where("g.group_id={$group_id}")
					->select();
		}
		return array();
	}
	
	/**
	 * 屏幕id列表
	 */
	public function group_screens($group_id){
		if($group_id){
			return $this
					->table("player_group_screen")
					->where("group_id={$group_id}")
					->getField("screen_id", true);
		}
		return array();
	}
}