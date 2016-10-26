<?php
namespace Index\Controller;

class IndexController extends CommonController {
    public function index(){
    	if(is_administrator()){
			$identity = 0;
		}else{
			if(D("User")->is_root(ADMIN_UID)){
				$identity = 1;
			}else if(D("User")->is_agent(ADMIN_UID)){
				$identity = 2;
			}else if(D("User")->is_normal(ADMIN_UID)){
				$identity = 3;
			}else{
				$identity = 4;
			}
		}
        $this->meta_title = '管理首页';
		$this->assign("index_identity", $identity);
        $this->display();
    }
    
    public function child_city($id=0){
    	$region_model = D("Region");
    	$citys = array();
    	if($id){
    		$citys = $region_model->all_region($id);
    	}
    	$this->success($citys);
    }
	
	public function reset_pwd(){
		if(IS_POST){
			$user_model = D("User");
			$rules = array(
				array('old','require','原密码不能为空！'),
				array('password','require','新密码不能为空！'),
				array('repassword','require','确认密码不能为空！'),
				array('password', "/^[A-Za-z0-9]{6,16}$/", '新密码格式错误！'),
				array('repassword','password','两次输入密码不一致！',0,'confirm')
			);
			if($user_model->validate($rules)->create()){
				$info = $user_model->user_by_id(ADMIN_UID, 'password');
				$old = I("post.old");
				$password = I("post.password");
				$db_pwd = $info['password'];
				if(sp_compare_password($old, $db_pwd)){
					$data = array();
					$data['uid'] = ADMIN_UID;
					$data['password'] = sp_password($password);
					$reset = $user_model->save($data);
					if($reset !== false){
						$this->success("密码修改成功！", U('index'));
					}else{
						$this->error("密码修改失败！");
					}
				}else{
					$this->error("原密码输入错误！");
				}
			}else{
				$this->error($user_model->getError());
			}
		}else{
			$this->meta_title = "修改密码";
			$this->display();
		}
	}

	public function information(){
		if(IS_POST){
			$user_model = D("User");
			$rules = array(
				array('uid','require','用户ID不能为空！'),
				array('email','require','邮箱不能为空！'),
				array('email','email','邮箱格式错误！'),
				array('email','','邮箱已经存在！',0,'unique',2),
				array('phone', "/^(0|86|17951)?(13[0-9]|15[012356789]|17[678]|18[0-9]|14[57])[0-9]{8}$/", '手机号码格式错误！'),
				array('phone','','手机号码已经存在！',2,'unique',2)
				
			);
			if($user_model->validate($rules)->create()){
				$data = array();
				$data['uid']		= I("post.uid");
				$data['email']		= I("post.email");
				$data['phone']		= I("post.phone", "");
				$data['realname']	= I("post.realname", "");
				$data['address']	= I("post.address", "");
				$res = $user_model->save($data);
				if($res !== false){
					if(session('is_full') == 1){
						session('is_full', 0);
					}
					$auth = array(
			            'uid'		=> $data['uid'],
			            'email'		=> $data['email'],
			            'lasttime'	=> NOW_TIME
			        );
					session('user_auth', $auth);
        			session('user_auth_sign', data_auth_sign($auth));
					$this->success('修改成功');
				}else{
					$this->error('修改失败');
				}
			}else{
				$this->error($user_model->getError());
			}
		}else{
			$user_model = D("User");
			$info = $user_model->user_by_id(ADMIN_UID);
			$this->assign("info", $info);
			$this->meta_title = "修改个人资料";
			$this->display();
		}
	}
	
	/**
	 * 代理商注册码
	 */
	public function agent_code(){
		if(IS_POST){
			$user_model = D("User");
			$rules = array(
				array('reg_code','require','注册码不能为空！'),
				array('reg_code', "/^[A-Za-z0-9]{12}$/",'格式错误：注册码为12位字母数字组合！'),
				array('reg_code','check_regcode','注册码已存在！',1,'function')
			);
			if($user_model->validate($rules)->create()){
				$data = array();
				$data['uid'] = ADMIN_UID;
				$data['reg_code'] = I("post.reg_code");
				$res = $user_model->save($data);
				if($res !== false){
					$this->success('修改成功');
				}else{
					$this->error('修改失败');
				}
			}else{
				$this->error($user_model->getError());
			}
		}else{
			$user_model = D("User");
			if($user_model->is_agent(ADMIN_UID)){
				$info = $user_model->user_by_id(ADMIN_UID, "reg_code");
				$this->assign("reg_code", $info['reg_code']);
				$this->meta_title = "代理商注册码";
				$this->display();
			}else{
				$this->error("系统错误：不是代理商！");
			}
		}
	}
}