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
		$map['email|phone'] = $email;
		$map['type'] = 2;
		$map['status'] = 0;
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
			if($temp['expire'] >= NOW_TIME){
				//未过期
				return $temp['uid'];
			}else{
				//已过期
				return 0;
			}
		}else{
			return false;
		}
	}
	
	/**
	 * 根据uid获取用户
	 * @param int $uid 用户uid
	 * @param string $field 查询字段
	 * @return array
	 */
	public function user_by_uid($uid, $field="*"){
		return $this->field($field)->find($uid);
	}
	
	/**
	 * 根据token获取用户
	 * @param string $token 登录token
	 * @param string $field 查询字段
	 * @return array
	 */
	public function user_by_token($token, $field="*"){
		$map = array();
		$map['token'] = $token;
		return $this->field($field)->where($map)->find();
	}
}
