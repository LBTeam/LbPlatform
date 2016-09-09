<?php
/**
 * 用户model
 * @author liangjian
 * @email 15934854815
 */
namespace Index\Model;
use Think\Model;

class UserModel extends Model
{
	/**
	 * 用户登录认证
	 * @param  string  $username 用户名
	 * @param  string  $password 用户密码
	 * @return integer 登录成功-用户ID，登录失败-错误编号
	 */
	public function login($username, $password){
		$map = array();
		$map['email'] = $username;  
        
		/* 获取用户数据 */
		$user = $this->where($map)->find();
        
		if($user){
			/* 验证用户密码 */
			if(sp_compare_password($password, $user['password'])){
                //登录成功           
                $uid = $user['uid'];
                // 更新登录信息 
                $this->autoLogin($user);
				return $uid ; //登录成功，返回用户UID
			} else {
				return -2; //密码错误
			}
		} else {
			return -1; //用户不存在
		}
	}
	
	/**
     * 注销当前用户
     * @return void
     */
    public function logout(){
    	session(C("USER_AUTH_KEY"), null);
		session(C("ADMIN_AUTH_KEY"), null);
        session('user_auth', null);
        session('user_auth_sign', null);
    }
	
	/**
     * 自动登录用户
     * @param  integer $user 用户信息数组
     */
    private function autoLogin($user){
        /* 更新登录信息 */
        $data = array(
            'uid'		=> $user['uid'],
            'lasttime'	=> NOW_TIME,
            'lastip'	=> get_client_ip(),
        );
        $this->save($data);

        /* 记录登录SESSION和COOKIES */
        $auth = array(
            'uid'		=> $user['uid'],
            'email'		=> $user['email'],
            'lasttime'	=> $user['lasttime'],
        );
		if(is_administrator()){
			session(C("ADMIN_AUTH_KEY"), $user['uid']);
		}
		session(C("USER_AUTH_KEY"), $user['uid']);
        session('user_auth', $auth);
        session('user_auth_sign', data_auth_sign($auth));
    }
}
