<?php
/**
 * 访问权限model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Model;
use Think\Model;

class AccessModel extends Model
{
	/**
	 * 用户组权限
	 */
	public function access_by_role($role_id){
		if($role_id){
			return $this->where("role_id={$role_id}")->getField("node_id", true);
		}
		return array();
	}
	
	/**
	 * 根据role_id删除权限
	 * @param $role_id 角色ID
	 */
	public function del_by_role($role_id){
		return $this->where("role_id = {$role_id}")->delete();
	}
	
	/**
	 * 添加多行权限
	 * @param $data 权限数据
	 */
	public function add_multiples($data){
		return $this->addAll($data);
	}
}
