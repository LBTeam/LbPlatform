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
}
?>