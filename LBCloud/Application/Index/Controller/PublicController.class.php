<?php
/**
 * Public控制器
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Controller;
use Think\Controller;

class PublicController extends Controller
{
	protected function _initialize(){
		$configs = D("Config")->configs();
		$this->assign("configs", $configs);
	}
	/**
     * 后台用户登录
     */
	public function login($username = null, $password = null, $verify = null){
		if(IS_POST){
			if(C('VERIFY_ENABLE')){
				if(!check_verify($verify)){
	                $this->error('验证码输入错误！');
	            } 
			}
            $user_model = D('User');
            $uid = $user_model->login($username, $password);            
            
            if(0 < $uid){ // 登录成功，$uid 为登录的 UID
                //跳转到登录前页面
                $this->success('登录成功！', U('Index/index'));
            } else { //登录失败
                switch($uid) {
                    case -1: $error = '用户不存在或已禁用！'; break; //系统级别禁用
                    case -2: $error = '密码错误！'; break;
                    default: $error = '未知错误！'; break; // 0-接口参数错误
                }
                $this->error($error);
            }
        } else {
            if(is_login()){
                $this->redirect(MODULE_PATH.'Index/index');
            }else{
                $this->display();
            }
        }
	}
	
	//退出登录 ,清除 session
    public function logout(){
        if(is_login()){
            D('User')->logout();
            session('[destroy]');
            //$this->success('退出成功！', U('Index/Public/login'));
            $this->redirect('login');
        } else {
            $this->redirect('login');
        }
    }
	
	/**
     * 生成 验证码
     */
    public function verify(){
        $verify = new \Think\Verify();
        $verify->entry(1);
    }
    
    /**
     * 注册
     */
    public function register($mode=1){
    	if(is_login()){
            $this->redirect('Index/Index/index');
        }else{
	    	if(IS_POST){
	    		if($mode == 1){
	    			$rules = array(
		    			array('phone','require','手机号码不能为空！'),
		    			array('phone', "/^(0|86|17951)?(13[0-9]|15[012356789]|17[678]|18[0-9]|14[57])[0-9]{8}$/", '手机号码格式错误！'),
						array('phone','','手机号码已经存在！',0,'unique',1),
						array('code','require','短信验证码不能为空！'),
						array('password','require','密码不能为空！'),
						array('password', "/^[A-Za-z0-9]{6,16}$/", '密码格式错误！'),
						array('code','check_smscode','短信验证码错误！',0,'function'),
						array('reg_code','check_agent_code','代理商注册码不存在！',2,'function'),
						array('repassword','password','两次输入密码不一致！',0,'confirm'),
					);
					if(C('VERIFY_ENABLE')){
			            $rules[] = array('verify','check_verify','验证码输入错误！',0,'function');
					}
					$user_model = D("User");
		    		if($user_model->validate($rules)->create()){
		    			$data = array();
		    			$data['password'] = sp_password(I("post.password"));
		    			$data['phone'] = I("post.phone", "");
		    			$data['type'] = 2;
		    			$data['is_full'] = 1;
		    			$data['lasttime'] = NOW_TIME;
		    			$data['lastip'] = get_client_ip();
		    			$data['addtime'] = NOW_TIME;
		    			$reg_code = I("post.reg_code", "");
		    			if($reg_code){
		    				$agent = $user_model->agent_by_regcode($reg_code, "uid");
		    				$data['puid'] = $agent['uid'];
		    				$data['status'] = 0;
		    			}else{
		    				$data['puid'] = 0;
		    				$data['status'] = 2;
		    			}
		    			$model = new \Think\Model();
		    			$model->startTrans();
		    			$uid = $user_model->add($data);
		    			$bind = true;
		    			if($reg_code){
	    					$cfg_model = D("Config");
	    					$role_model = D("Role");
	    					$roles = $cfg_model->roles();
	    					$bind = $role_model->bind_relation($uid, $roles['normal']);
	    				}
		    			if($uid && $bind){
		    				$model->commit();
		    				//注册成功
		    				$auth = array(
					            'uid'		=> $uid,
					            'phone'		=> $data['phone'],
					            'lasttime'	=> NOW_TIME,
					        );
					        session(C("USER_AUTH_KEY"), $uid);
					        session('is_full', 1);
					        session('user_auth', $auth);
					        session('user_auth_sign', data_auth_sign($auth));
					        $this->success("注册成功！", U('Index/Index/information'));
		    			}else{
		    				//注册失败
		    				$model->rollback();
		    				$this->error("注册失败");
		    			}
		    		}else{
		    			$this->error($user_model->getError());
		    		}
	    		}else{
	    			//邮箱注册 
	    			$rules = array(
		    			array('email','require','注册邮箱不能为空！'),
		    			array('email', "email", '注册邮箱格式错误！'),
						array('email','','注册邮箱已经存在！',0,'unique',1),
						array('password','require','密码不能为空！'),
						array('password', "/^[A-Za-z0-9]{6,16}$/", '密码格式错误！'),
						array('reg_code','check_agent_code','代理商注册码不存在！',2,'function'),
						array('repassword','password','两次输入密码不一致！',0,'confirm'),
					);
					if(C('VERIFY_ENABLE')){
			            $rules[] = array('verify','check_verify','验证码输入错误！',0,'function');
					}
					$user_model = D("User");
		    		if($user_model->validate($rules)->create()){
		    			$data = array();
						$data['email'] = I("post.email", "");
						$data['code'] = I("post.reg_code", "");
						$data['_pw'] = I("post.password", "");
						$data['sign'] = create_data_sign($data);
						$uri = "http://".$_SERVER['HTTP_HOST'].U("Index/Public/dofull")."?".http_build_query($data);
						$email_content = "";
						$email_content .= "尊敬的用户，你好：<br>";
						$email_content .= "&nbsp;&nbsp;请点击<a href='{$uri}'>{$uri}</a>激活账号！";
						$m_info = array();
						$m_info['adder'] = $data['email'];
						$m_info['title'] = "注册邮件";
						$m_info['content'] = $email_content;
						$request_uri = C("EMAIL_SERVER").http_build_query($m_info);
						$resp = file_get_contents($request_uri);
						if($resp === ''){
							session("send_email", $data['email']);
							$this->success("注册成功", U('Index/Public/send_ok'));
						}else{
							$this->error("注册失败");
						}
		    		}else{
		    			$this->error($user_model->getError());
		    		}
	    		}
	    	}else{
	    		$template = "register_{$mode}";
	    		$this->display($template);
	    	}
	    }
    }
    
    /**
     * 发送手机验证码
     */
    public function send_code($phone){
    	if(empty($phone) === true){
    		$this->error("手机号码不能为空！");
    	}else{
    		$reg_string = "/^(0|86|17951)?(13[0-9]|15[012356789]|17[678]|18[0-9]|14[57])[0-9]{8}$/";
    		if(!preg_match($reg_string, $phone)){
    			$this->error("手机号码格式错误！");
    		}else{
    			$last_time = session("{$phone}_time");
    			$last_time = $last_time ? $last_time : 0;
    			if($last_time &&  NOW_TIME - $last_time < 120){
    				$sec = 120 - (NOW_TIME - $last_time);
    				$this->error("{$sec}秒后可在次获取！");
    			}else{
    				$code = random_string(4, true);
		    		$params = array();
		    		$params['phones'] = $phone;
		    		$params['msg'] = "Lbcloud管理系统验证码：{$code}";
		    		$request_param = http_build_query($params);
		    		$request_uri = C("SMS_SERVER") . $request_param . "&returnType=api";
		    		$resp = file_get_contents($request_uri);
		    		if($resp == 'success'){
		    			session("{$phone}_code", $code);
		    			session("{$phone}_time", NOW_TIME);
		    			$this->success("短信发送成功！");
		    		}else{
		    			$this->error("短信发送失败！");
		    		}
    			}
    		}
    	}
    }

	/**
	 * 邮件发送成功
	 */
	public function send_ok(){
		$email = session("send_email");
		if($email){
			$this->assign("mail", $email);
			$this->display();
		}else{
			$this->error("系统错误：非法操作！", U('Index/Index/index'));
		}
	}
	
	/**
	 * 完善资料
	 */
	public function dofull(){
		if(IS_POST){
			$rules = array(
				array('email','require','邮箱不能为空！'),
				array('email','email','邮箱格式错误！'),
				array('email','','邮箱已经存在！',0,'unique',1),
				array('phone','require','手机号码不能为空！'),
				array('phone', "/^(0|86|17951)?(13[0-9]|15[012356789]|17[678]|18[0-9]|14[57])[0-9]{8}$/", '手机号码格式错误！'),
				array('phone','','手机号码已经存在！',2,'unique',1),
				array('realname','require','真实姓名不能为空！'),
				array('address','require','地址不能为空！'),
				array('password','require','密码不能为空！'),
				array('reg_code','check_agent_code','代理商注册码不存在！',2,'function')
			);
			$user_model = D("User");
			if($user_model->validate($rules)->create()){
				$data = array();
				$data['email'] = I("post.email");
    			$data['password'] = sp_password(I("post.password"));
    			$data['phone'] = I("post.phone", "");
				$data['realname'] = I("post.realname");
				$data['address'] = I("post.address");
    			$data['type'] = 2;
    			$data['is_full'] = 2;
    			$data['lasttime'] = NOW_TIME;
    			$data['lastip'] = get_client_ip();
    			$data['addtime'] = NOW_TIME;
    			$reg_code = I("post.reg_code", "");
    			if($reg_code){
    				$agent = $user_model->agent_by_regcode($reg_code, "uid");
    				$data['puid'] = $agent['uid'];
    				$data['status'] = 0;
    			}else{
    				$data['puid'] = 0;
    				$data['status'] = 2;
    			}
				$model = new \Think\Model();
    			$model->startTrans();
    			$uid = $user_model->add($data);
    			$bind = true;
    			if($reg_code){
					$cfg_model = D("Config");
					$role_model = D("Role");
					$roles = $cfg_model->roles();
					$bind = $role_model->bind_relation($uid, $roles['normal']);
				}
    			if($uid && $bind){
    				$model->commit();
    				//注册成功
    				$auth = array(
			            'uid'		=> $uid,
			            'email'		=> $data['email'],
			            'lasttime'	=> NOW_TIME,
			        );
			        session("send_email", null);
			        
			        session(C("USER_AUTH_KEY"), $uid);
			        session('is_full', 2);
			        session('user_auth', $auth);
			        session('user_auth_sign', data_auth_sign($auth));
			        //成功后处理
			        $this->success("资料完善成功", U('Index/Index/index'));
    			}else{
    				//注册失败
    				$model->rollback();
    				$this->error("完善资料失败", U('Index/Index/index'));
    			}
			}else{
				$this->error($user_model->getError());
			}
		}else{
			$data = array();
			$data['email'] = I("get.email", "");
			$data['code'] = I("get.code", "");
			$data['_pw'] = I("get._pw", "");
			$sign = create_data_sign($data);
			$get_sign = I("get.sign");
			if($sign == $get_sign){
				$user_model = D("User");
				$user = $user_model->user_by_email($data['email']);
				if($user){
					$this->error("确认邮箱链接已失效！", U('Index/Index/index'));
				}else{
					$this->meta_title = "资料完善";
					$this->assign("data", $data);
					$this->display();
				}
			}else{
				$this->error("系统错误：非法操作！", U('Index/Index/index'));
			}
		}
	}

	public function find($mode=1){
		if(is_login()){
            $this->redirect('Index/Index/index');
        }else{
			if(IS_POST){
				if($mode == 1){
					//手机找回
					$rules = array(
		    			array('phone','require','手机号码不能为空！',1),
		    			array('phone', "/^(0|86|17951)?(13[0-9]|15[012356789]|17[678]|18[0-9]|14[57])[0-9]{8}$/", '手机号码格式错误！',1),
		    			array('phone','check_phone','手机号码！',1,'function'),
						array('code','require','短信验证码不能为空！',1),
						array('code','check_smscode','短信验证码错误！',1,'function'),
						array('verify','check_verify','验证码输入错误！',0,'function')
					);
					$user_model = D("User");
					if($user_model->validate($rules)->create()){
						session("find_mobile", I("post.phone"));
						$this->success("验证通过", U("Index/Public/reset"));
					}else{
						$this->error($user_model->getError());
					}
				}else{
					//邮箱找回
					$rules = array(
		    			array('email','require','邮箱不能为空！',1),
		    			array('email', "email", '邮箱格式错误！',1),
		    			array('email', "check_email", '邮箱不存在！',1,'function'),
		    			array('verify','check_verify','验证码输入错误！',0,'function')
		    		);
		    		$user_model = D("User");
		    		if($user_model->validate($rules)->create()){
		    			$data = array();
						$data['email'] = I("post.email", "");
						$data['timestamp'] = NOW_TIME + 600;
						$data['sign'] = create_data_sign($data);
						$back_uri = "http://".$_SERVER['HTTP_HOST'].U("Index/Public/reset")."?".http_build_query($data);
						$email_content = "";
						$email_content .= "尊敬的用户，你好：<br>";
						$email_content .= "&nbsp;&nbsp;请点击<a href='{$back_uri}'>{$back_uri}</a>重置密码！";
						$m_info = array();
						$m_info['adder'] = $data['email'];
						$m_info['title'] = "LbCloud管理系统-重置密码";
						$m_info['content'] = $email_content;
						$request_uri = C("EMAIL_SERVER").http_build_query($m_info);
						$resp = file_get_contents($request_uri);
						if($resp === ''){
							session("reset_email", $data['email']);
							$this->success("发送成功", U('Index/Public/email_ok'));
						}else{
							$this->error("邮件发送失败");
						}
		    		}else{
						$this->error($user_model->getError());
					}
				}
			}else{
				$template = "find_{$mode}";
				$this->display($template);
			}
		}
	}

	public function reset(){
		if(IS_POST){
			$rules = array(
				array('password','require','新密码不能为空！'),
				array('password', "/^[A-Za-z0-9]{6,16}$/", '密码格式错误！'),
				array('repassword','password','两次输入密码不一致！',0,'confirm')
			);
			$user_model = D("User");
			if($user_model->validate($rules)->create()){
				$mobile = session("find_mobile");
				if($mobile){
					$map = array("phone" => $mobile);
					$data = array(
						"password" => sp_password(I("post.password"))
					);
					$res = $user_model->where($map)->save($data);
					if($res !== false){
						session("find_mobile", null);
						$this->success("密码修改成功！", U("Index/Public/login"));
					}else{
						$this->error("密码修改失败！");
					}
				}else{
					$email = session("reset_email");
					if($email){
						$map = array("email" => $email);
						$data = array(
							"password" => sp_password(I("post.password"))
						);
						$res = $user_model->where($map)->save($data);
						if($res !== false){
							session("reset_email", null);
							$this->success("密码修改成功！", U("Index/Public/login"));
						}else{
							$this->error("密码修改失败！");
						}
					}else{
						$this->error("系统错误：非法操作！");
					}
				}
			}else{
				$this->error($user_model->getError());
			}
		}else{
			$mobile = session("find_mobile");
			if($mobile){
				$this->display();
			}else{
				$email = I("get.email", "");
				$timestamp = I("get.timestamp", "");
				if($email && $timestamp){
					if($timestamp >= NOW_TIME){
						$data = array();
						$data['email'] = $email;
						$data['timestamp'] = $timestamp;
						$sign = I("get.sign", "");
						$d_sign = create_data_sign($data);
						if($sign == $d_sign){
							session("reset_email", $email);
							$this->display();
						}else{
							$this->error("系统错误：非法操作！", U('Index/Index/index'));
						}
					}else{
						$this->error("重置密码链接已失效！", U('Index/Index/index'));
					}
				}else{
					$this->error("系统错误：非法操作！", U('Index/Index/index'));
				}
			}
		}
	}
	
	/**
	 * 重置密码邮件发送成功
	 */
	public function email_ok(){
		$email = session("reset_email");
		if($email){
			$this->assign("mail", $email);
			$this->display();
		}else{
			$this->error("系统错误：非法操作！", U('Index/Index/index'));
		}
	}
}
