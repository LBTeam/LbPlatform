<?php
/**
 * 用户角色管理model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Model;
use Think\Model;

class RoleModel extends Model
{
	/**
	 * 用户组列表
	 */
	public function all_role_list(){
		return $this->select();
	}
	
	/**
	 * 用户组详情
	 */
	public function role_by_id($id, $field="*"){
		if($id){
			return $this->field($field)->find($id);
		}
		return array();
	}
	
	/**
	 * 用户组ID
	 */
	public function role_id_by_user($uid){
		if($uid){
			return $this->table("player_role_user")
						->where("user_id={$uid}")
						->getField("role_id");
		}
		return 0;
	}
	
	/**
	 * 绑定用户关系
	 */
	public function bind_relation($user_id, $role_id){
		$this->table('player_role_user')
			 ->where("user_id={$user_id}")
			 ->delete();
		$query = "INSERT INTO `player_role_user` ";
		$query .= "VALUES ({$role_id}, {$user_id});";
		return $this->execute($query);
	}
	
	/**
	 * 接触绑定
	 */
	public function unbind_relation($user_id=0, $role_id=0){
		$map = array();
		if($user_id){
			$map['user_id'] = array("IN", $user_id);
		}
		if($role_id){
			$map['role_id'] = array("IN", $role_id);
		}
		return $this->table('player_role_user')->where($map)->delete();
	}
	
	/**
	 * 用户组用户列表
	 */
	public function users_by_role($role_id){
		if($role_id){
			$map = array();
			$map['role.role_id'] = $role_id;
			$field = "role.role_id,user.uid,user.email,user.phone,user.realname,user.status";
			$field .= ",user.puid,user.lasttime,user.lastip,user.addtime,user.reg_code";
			$users = $this->table("player_role_user")
						  ->alias("role")
						  ->field($field)
						  ->join("player_user AS user ON user.uid = role.user_id")
						  ->where($map)
						  ->select();
			return $users;
		}
		return array();
	}
}