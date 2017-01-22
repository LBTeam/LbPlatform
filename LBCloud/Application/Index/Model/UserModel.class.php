<?php
/**
 * 用户model
 * @author liangjian
 * @email 15934854815@163.com
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
		$map['email|phone'] = $username;  
		$map['status'] = 0;
        
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
		session('is_full', null);
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
		session('is_full', $user['is_full']);
        session('user_auth', $auth);
        session('user_auth_sign', data_auth_sign($auth));
    }
    
    /**
     * 用户列表
     */
    public function all_user_list(){
    	$field = "uid,email,phone,realname,address,status";
    	$field .= ",type,lasttime,lastip,addtime,reg_code";
    	if(is_administrator()){
    		$users = $this->field($field)->select();
    	}else{
    		$role_model = D("Role");
    		$cfg_model = D("Config");
    		$uid = session("user_auth.uid");
    		$role_id = $role_model->role_id_by_user($uid);
    		$roles = $cfg_model->roles();
    		switch($role_id){
    			case $roles['root']:
    				//管理员
    				$users = $this->field($field)->select();
    				break;
    			case $roles['agent']:
    				//代理用户
    				$map = array("puid"=>$uid);
    				$users = $this->field($field)->where($map)->select();
    				break;
    			case $roles['normal']:
    				//普通用户
    				$users = array();
    				break;
    			default:
    				$users = array();
    				break;
    		}
    	}
    	return $users;
    }
    
    /**
     * 管理员列表
     */
    public function root_list(){
    	$map = array();
    	$users = array();
    	$field = "uid,email,phone,realname,address";
    	$field .= ",status,lasttime,lastip,addtime";
    	if(is_administrator()){
    		$map['type'] = 0;
    	}else{
    		$map['type'] = 0;
    		$map['uid'] = array("NEQ", C("USER_ADMINISTRATOR"));
    	}
    	$users = $this->field($field)->where($map)->select();
    	return $users;
    }
    
    /**
     * 代理商列表
     */
	public function agent_list(){
		$map = array();
		$map['type'] = 1;
		$map['is_del'] = 0;
    	$field = "uid,email,phone,realname,address,status";
    	$field .= ",lasttime,lastip,addtime,reg_code";
    	return $this->field($field)->where($map)->select();
	}
	
	/**
	 * 代理商注册码
	 */
	public function agent_regcode(){
		$code = random_string(12);
		$code_list = $this
						->where('reg_code != ""')
						->getField("reg_code", true);
		while(in_array($code, $code_list)){
			$code = random_string(12);
		}
		return $code;
	}
    
    /**
     * 普通用户列表
     */
    public function user_list(){
    	$map = array();
    	$users = array();
    	$field = "uid,email,phone,realname,address";
    	$field .= ",puid,status,lasttime,lastip,addtime";
    	if(is_administrator()){
    		$map['type'] = 2;
    	}else{
    		if($this->is_root(session("user_auth.uid"))){
    			$map['type'] = 2;
    		}else{
    			$uid = session("user_auth.uid");
	    		$map['type'] = 2;
	    		$map['puid'] = $uid;
    		}
    	}
    	$users = $this->field($field)->where($map)->select();
    	$agents = $this->all_agents();
    	foreach ($users as $key => &$value) {
    		$value['p_email'] = $agents[$value['puid']]['email'];
    		$value['p_phone'] = $agents[$value['puid']]['phone'];
    	}
    	return $users;
    }
    
    /**
     * 所有代理商及邮件和电话
     */
    public function all_agents(){
    	return $this->where("type=1")->getField("uid,email,phone");
    }
    
    /**
     * 用户详情
     */
    public function user_by_id($uid, $field="*"){
    	if($uid){
    		return $this->field($field)->find($uid);
    	}
    	return array();
    }
    
    /**
     * 下级用户列表
     */
    public function users_by_puid($puid){
    	$map = array();
    	$map['type'] = 2;
    	$map['is_del'] = 0;
    	if(!is_administrator()){
    		$cfg_model = D("Config");
    		$roles = $cfg_model->roles();
    		if($this->is_normal($puid, 0, $roles)){
    			return array();
    		}elseif($this->is_agent($puid, 0, $roles)){
    			$map['puid'] = $puid;
    		}
    	}
    	return $this->where($map)->getField("uid,email,phone");
    }
    
    /**
     * 是否是管理员
     */
    public function is_root($uid=0, $role_id=0, $roles=array()){
    	if(!$roles){
    		$cfg_model = D("Config");
    		$roles = $cfg_model->roles();
    	}
    	if(!$role_id){
    		$role_model = D("Role");
    		$role_id = $role_model->role_id_by_user($uid);
    	}
    	return $role_id && ($role_id == $roles['root']);
    }
    
    /**
     * 是否是代理商
     */
    public function is_agent($uid=0, $role_id=0, $roles=array()){
    	if(!$roles){
    		$cfg_model = D("Config");
    		$roles = $cfg_model->roles();
    	}
    	if(!$role_id){
    		$role_model = D("Role");
    		$role_id = $role_model->role_id_by_user($uid);
    	}
    	return $role_id && ($role_id == $roles['agent']);
    }
    
    /**
     * 是否是普通用户
     */
    public function is_normal($uid=0, $role_id=0, $roles=array()){
    	if(!$roles){
    		$cfg_model = D("Config");
    		$roles = $cfg_model->roles();
    	}
    	if(!$role_id){
    		$role_model = D("Role");
    		$role_id = $role_model->role_id_by_user($uid);
    	}
    	return $role_id && ($role_id == $roles['normal']);
    }
    
    /**
     * 根据注册码获取代理商数据
     */
    public function agent_by_regcode($reg_code, $field="*"){
    	$map = array("reg_code" => $reg_code);
    	return $this->field($field)->where($map)->find();
    }
	
	/**
     * 根据邮箱获取用户数据
     */
	public function user_by_email($email, $field="*"){
		$map = array("email" => $email);
    	return $this->field($field)->where($map)->find();
	}
	
	/**
	 * 用户list用户手机号列表
	 */
	public function user_mobiles($uids = array()){
		if($uids){
			$map = array();
			$map['uid'] = array("IN", $uids);
			return $this->where($map)->getField("phone", true);
		}
		return array();
	}
	
	/**
	 * 用户list邮箱地址列表
	 */
	public function user_emails($uids = array()){
		if($uids){
			$map = array();
			$map['uid'] = array("IN", $uids);
			return $this->where($map)->getField("email", true);
		}
		return array();
	}
	
	/**
	 * 根据用户ID获取上级代理商信息
	 */
	public function agent_by_uid($user_id){
		$result = array();
		$puid = $this->where("uid = {$user_id}")->getField("puid");
		if($puid){
			$map = array();
			$map['uid'] = $puid;
			$map['type'] = 1;
			$result = $this->where($map)->find();
		}
		return $result;
	}
}
