<?php
/**
 * 用户model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Api\Model;
use Think\Model;

class UserModel extends Model
{
	/**
	 * 根据email获取用户
	 * @param string $email 邮箱
	 * @return array
	 */
	public function user_by_email($email){
		$map = [];
		$map['email'] = $email;
		$map['type'] = 2;
		return $this->where($map)->find();
	}
}
