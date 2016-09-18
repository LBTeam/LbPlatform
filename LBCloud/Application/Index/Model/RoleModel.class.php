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
}
?>