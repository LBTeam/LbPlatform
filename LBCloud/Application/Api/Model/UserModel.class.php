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
		$map = array();
		$map['email'] = $email;
		$map['type'] = 2;
		return $this->where($map)->find();
	}
	
	/**
	 * 检查token
	 * @param $token 登录token
	 * @return boolen|int
	 */
	public function check_token($token){
		$map = array();
		$map['token'] = $token;
		$temp = $this->where($map)->find();
		if($temp){
			if($temp['expire'] < time()){
				return $temp['uid'];
			}else{
				return 0;
			}
		}else{
			return false;
		}
	}
}
