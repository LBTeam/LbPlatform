<?php
/**
 * 屏幕model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Model;
use Think\Model;

class ScreenModel extends Model
{
	/**
	 * 用户屏体列表
	 */
	public function screen_by_uid($user_id){
		$map = array();
		$map['s.is_delete'] = 0;
		if(!is_administrator()){
			$cfg_model = D("Config");
			$user_model = D("User");
			$roles = $cfg_model->roles();
			if($user_model->is_agent($user_id, 0, $roles)){
				$map['u.puid'] = $user_id;
			}elseif($user_model->is_normal($user_id, 0, $roles)){
				$map['s.uid'] = $user_id;
			}
		}
		return $this
				->alias("s")
				->field("s.*,p.bind_id,p.bind_key,g.id AS group_id,g.name AS group_name,u.email AS u_email,u.phone AS u_phone")
				->join("player_group_screen AS rela ON rela.screen_id = s.id AND rela.uid = {$user_id}", 'LEFT')
				->join("player_group AS g ON rela.group_id = g.id", 'LEFT')
				->join("player_player AS p ON p.id = s.id", 'LEFT')
				->join("player_user AS u ON u.uid = s.uid")
				->where($map)
				->select();
	}
	
	/**
	 * 屏幕详情
	 */
	public function screen_by_id($id, $field = "s.*,g.group_id"){
		if($id){
			$uid = ADMIN_UID;
			return $this
					->alias("s")
					->field($field)
					->join("player_group_screen AS g ON g.screen_id = s.id AND g.uid = {$uid}", 'LEFT')
					->where("s.id={$id}")
					->find();
		}
		return array();
	}
	
	/**
	 * 绑定屏幕分组关系
	 */
	public function bind_group($uid, $screen_id, $group_id){
		$sql = "INSERT INTO `player_group_screen` (`uid`,`screen_id`,`group_id`)";
		$sql .= " VALUES ({$uid}, {$screen_id}, {$group_id})";
		return $this->execute($sql);
	}
	
	/**
	 * 解绑
	 */
	public function unbind_group($uid=0, $screen_id=0, $group_id=0){
		$map = array();
		if($uid){
			$map['uid'] = $uid;
		}
		if($screen_id){
			$map['screen_id'] = array('IN', $screen_id);
		}
		if($group_id){
			$map['group_id'] = array('IN', $group_id);
		}
		return $this->table("player_group_screen")->where($map)->delete();
	}
}
