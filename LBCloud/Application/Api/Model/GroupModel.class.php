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
}
