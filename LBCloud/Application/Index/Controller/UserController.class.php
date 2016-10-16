<?php
/**
 * 用户管理控制器
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Controller;

class UserController extends CommonController {
	/**
	 * 管理员列表
	 */
	public function root_list(){
		$user_model = D("User");
		$users = $user_model->root_list();
		$condition = array(
    		"status" => array(
    			0 => "正常",
    			1 => "禁用"
    		)
    	);
    	int_to_string($users, $condition);
    	$this->assign("users", $users);
    	$this->meta_title = '管理员列表';
		$this->display();
	}
	
	/**
	 * 添加管理员
	 */
	public function add_root(){
		if(IS_POST){
			$user_model = D("User");
			$rules = array(
				array('email','require','邮箱不能为空！'),
				array('email','email','邮箱格式错误！'),
				array('email','','邮箱已经存在！',0,'unique',1),
				array('phone', "/^(0|86|17951)?(13[0-9]|15[012356789]|17[678]|18[0-9]|14[57])[0-9]{8}$/", '手机号码格式错误！'),
				array('phone','','手机号码已经存在！',2,'unique',1),
				array('password','require','密码不能为空'),
				array('password', "/^[A-Za-z0-9]{6,16}$/", '密码格式错误'),
				array('re_password','password','两次输入密码不一致',0,'confirm'), 
			);
			if($user_model->validate($rules)->create()){
				$data = array();
				$data['password']	= sp_password(I("post.password"));
				$data['email']		= I("post.email");
				$data['phone']		= I("post.phone", "");
				$data['realname']	= I("post.realname", "");
				$data['address']	= I("post.address", "");
				$data['puid']		= 0;
				$data['status']		= I("post.status", 0);
				$data['type']		= 0;
				$data['lasttime']	= 0;
				$data['lastip']		= '0.0.0.0';
				$data['addtime']	= NOW_TIME;
				$cfg_model = D("Config");
				$role_model = D("Role");
				$roles = $cfg_model->roles();
				$model = new \Think\Model();
				$model->startTrans();
				$user_id = $user_model->add($data);
				$bind_res = $role_model->bind_relation($user_id, $roles['root']);
				if($user_id && $bind_res){
					$model->commit();
					$this->success('新增成功', U('root_list'));
				}else{
					$model->rollback();
					$this->error('新增失败');
				}
			}else{
				$this->error($user_model->getError());
			}
		}else{
			$this->meta_title = '新增管理员';
            $this->display();
		}
	}
	
	/**
	 * 修改管理员信息
	 */
	public function edit_root($id = 0){
		$user_model = D("User");
		if(IS_POST){
			$rules = array(
				array('uid','require','用户ID不能为空！'),
				array('email','require','邮箱不能为空！'),
				array('email','email','邮箱格式错误！'),
				array('email','','邮箱已经存在！',0,'unique',2),
				array('phone', "/^(0|86|17951)?(13[0-9]|15[012356789]|17[678]|18[0-9]|14[57])[0-9]{8}$/", '手机号码格式错误！'),
				array('phone','','手机号码已经存在！',2,'unique',2),
				array('password', "/^[A-Za-z0-9]{6,16}$/",'密码格式错误！',2),
				array('re_password','password','两次输入密码不一致！',0,'confirm'), 
			);
			if($user_model->validate($rules)->create()){
				$data = array();
				$data['uid']		= I("post.uid");
				$data['email']		= I("post.email");
				$data['phone']		= I("post.phone", "");
				$data['realname']	= I("post.realname", "");
				$data['address']	= I("post.address", "");
				$data['status']		= I("post.status", 0);
				$password = I("post.password", "");
				if($password){
					$data['password'] = sp_password($password);
				}
				$res = $user_model->save($data);
				if($res !== false){
					$this->success('修改成功', U('root_list'));
				}else{
					$this->error('修改失败');
				}
			}else{
				$this->error($user_model->getError());
			}
		}else{
			$field = "uid,email,phone,realname,address,status";
			$info = $user_model->user_by_id($id, $field);
			if($info){
				$this->assign('info', $info);
				$this->meta_title = '修改管理员';
            	$this->display();
			}else{
				$this->error('获取管理员信息错误');
			}
		}
	}
	
	/**
	 * 代理商列表
	 */
	public function agent_list(){
		$user_model = D("User");
		$users = $user_model->agent_list();
		$condition = array(
    		"status" => array(
    			0 => "正常",
    			1 => "禁用"
    		)
    	);
    	int_to_string($users, $condition);
    	$this->assign("users", $users);
    	$this->meta_title = '代理商列表';
		$this->display();
	}
	
	/**
	 * 添加代理商
	 */
	public function add_agent(){
		if(IS_POST){
			$user_model = D("User");
			$rules = array(
				array('email','require','邮箱不能为空！'),
				array('email','email','邮箱格式错误！'),
				array('email','','邮箱已经存在！',0,'unique',1),
				array('phone', "/^(0|86|17951)?(13[0-9]|15[012356789]|17[678]|18[0-9]|14[57])[0-9]{8}$/", '手机号码格式错误！'),
				array('phone','','手机号码已经存在！',2,'unique',1),
				array('password','require','密码不能为空'),
				array('password', "/^[A-Za-z0-9]{6,16}$/", '密码格式错误'),
				array('re_password','password','两次输入密码不一致',0,'confirm'), 
			);
			if($user_model->validate($rules)->create()){
				$data = array();
				$data['password']	= sp_password(I("post.password"));
				$data['email']		= I("post.email");
				$data['phone']		= I("post.phone", "");
				$data['realname']	= I("post.realname", "");
				$data['address']	= I("post.address", "");
				$data['puid']		= 0;
				$data['status']		= I("post.status", 0);
				$data['type']		= 1;
				$data['lasttime']	= 0;
				$data['lastip']		= '0.0.0.0';
				$data['addtime']	= NOW_TIME;
				$data['reg_code']	= $user_model->agent_regcode();
				$cfg_model = D("Config");
				$role_model = D("Role");
				$roles = $cfg_model->roles();
				$model = new \Think\Model();
				$model->startTrans();
				$user_id = $user_model->add($data);
				$bind_res = $role_model->bind_relation($user_id, $roles['agent']);
				if($user_id && $bind_res){
					$model->commit();
					$this->success('新增成功', U('agent_list'));
				}else{
					$model->rollback();
					$this->error('新增失败');
				}
			}else{
				$this->error($user_model->getError());
			}
		}else{
			$this->meta_title = '新增代理商';
            $this->display();
		}
	}
	
	/**
	 * 修改代理商
	 */
	public function edit_agent($id = 0){
		$user_model = D("User");
		if(IS_POST){
			$rules = array(
				array('uid','require','用户ID不能为空！'),
				array('email','require','邮箱不能为空！'),
				array('email','email','邮箱格式错误！'),
				array('email','','邮箱已经存在！',0,'unique',2),
				array('phone', "/^(0|86|17951)?(13[0-9]|15[012356789]|17[678]|18[0-9]|14[57])[0-9]{8}$/", '手机号码格式错误！'),
				array('phone','','手机号码已经存在！',2,'unique',2),
				array('password', "/^[A-Za-z0-9]{6,16}$/",'密码格式错误！',2),
				array('re_password','password','两次输入密码不一致！',0,'confirm'), 
			);
			if($user_model->validate($rules)->create()){
				$data = array();
				$data['uid']		= I("post.uid");
				$data['email']		= I("post.email");
				$data['phone']		= I("post.phone", "");
				$data['realname']	= I("post.realname", "");
				$data['address']	= I("post.address", "");
				$data['status']		= I("post.status", 0);
				$password = I("post.password", "");
				if($password){
					$data['password'] = sp_password($password);
				}
				$res = $user_model->save($data);
				if($res !== false){
					$this->success('修改成功', U('agent_list'));
				}else{
					$this->error('修改失败');
				}
			}else{
				$this->error($user_model->getError());
			}
		}else{
			$field = "uid,email,phone,realname,address,status";
			$info = $user_model->user_by_id($id, $field);
			if($info){
				$this->assign('info', $info);
				$this->meta_title = '修改代理商';
            	$this->display();
			}else{
				$this->error('获取代理商信息错误');
			}
		}
	}

	
	/**
	 * 用户列表
	 */
    public function index(){
    	$user_model = D("User");
    	$users = $user_model->user_list();
    	$condition = array(
    		"status" => array(
    			0 => "正常",
    			1 => "禁用"
    		)
    	);
    	int_to_string($users, $condition);
    	if(is_administrator()){
			$this->assign("is_admin", 1);
		}else{
			$this->assign("is_admin", 0);
		}
    	$this->assign("users", $users);
    	$this->meta_title = '用户列表';
		$this->display();
    }
    
    /**
     * 添加用户
     */
    public function add(){
    	if(IS_POST){
    		$user_model = D("User");
			$rules = array(
				array('email','require','邮箱不能为空！'),
				array('email','email','邮箱格式错误！'),
				array('email','','邮箱已经存在！',0,'unique',1),
				array('phone', "/^(0|86|17951)?(13[0-9]|15[012356789]|17[678]|18[0-9]|14[57])[0-9]{8}$/", '手机号码格式错误！'),
				array('phone','','手机号码已经存在！',2,'unique',1),
				array('password','require','密码不能为空'),
				array('password', "/^[A-Za-z0-9]{6,16}$/", '密码格式错误'),
				array('re_password','password','两次输入密码不一致',0,'confirm')
			);
			if(is_administrator()){
				$rules[] = array('puid','require','请选择上级代理商');
			}
			if($user_model->validate($rules)->create()){
				$data = array();
				$data['password']	= sp_password(I("post.password"));
				$data['email']		= I("post.email");
				$data['phone']		= I("post.phone", "");
				$data['realname']	= I("post.realname", "");
				$data['address']	= I("post.address", "");
				$data['puid']		= I("post.puid") ? I("post.puid") : session("user_auth.uid");
				$data['status']		= I("post.status", 0);
				$data['type']		= 2;
				$data['lasttime']	= 0;
				$data['lastip']		= '0.0.0.0';
				$data['addtime']	= NOW_TIME;
				$cfg_model = D("Config");
				$role_model = D("Role");
				$roles = $cfg_model->roles();
				$model = new \Think\Model();
				$model->startTrans();
				$user_id = $user_model->add($data);
				$bind_res = $role_model->bind_relation($user_id, $roles['normal']);
				if($user_id && $bind_res){
					$model->commit();
					$this->success('新增成功', U('index'));
				}else{
					$model->rollback();
					$this->error('新增失败');
				}
			}else{
				$this->error($user_model->getError());
			}
    	}else{
    		if(is_administrator()){
    			$user_model = D("User");
    			$agents = $user_model->all_agents();
    			$this->assign("is_admin", 1);
    			$this->assign("agents", $agents);
    		}else{
    			$this->assign("is_admin", 0);
    		}
    		$this->meta_title = '新增普通用户';
            $this->display();
    	}
    }
    
    /**
     * 编辑用户
     */
    public function edit($id = 0){
    	$user_model = D("User");
    	if(IS_POST){
    		$rules = array(
				array('email','require','邮箱不能为空！'),
				array('email','email','邮箱格式错误！'),
				array('email','','邮箱已经存在！',0,'unique',2),
				array('phone', "/^(0|86|17951)?(13[0-9]|15[012356789]|17[678]|18[0-9]|14[57])[0-9]{8}$/", '手机号码格式错误！'),
				array('phone','','手机号码已经存在！',2,'unique',2),
				array('password', "/^[A-Za-z0-9]{6,16}$/", '密码格式错误',2),
				array('re_password','password','两次输入密码不一致',0,'confirm')
			);
			if(is_administrator()){
				$rules[] = array('puid','require','请选择上级代理商');
			}
			if($user_model->validate($rules)->create()){
				$data = array();
				$data['uid']		= I("post.uid");
				$data['email']		= I("post.email");
				$data['phone']		= I("post.phone", "");
				$data['realname']	= I("post.realname", "");
				$data['address']	= I("post.address", "");
				$data['puid']		= I("post.puid") ? I("post.puid") : session("user_auth.uid");
				$data['status']		= I("post.status", 0);
				$password = I("post.password", "");
				if($password){
					$data['password'] = sp_password($password);
				}
				$res = $user_model->save($data);
				if($res !== false){
					$this->success('修改成功', U('index'));
				}else{
					$this->error('修改失败');
				}
			}else{
				$this->error($user_model->getError());
			}
    	}else{
    		$field = "uid,email,phone,realname,address,puid,status";
			$info = $user_model->user_by_id($id, $field);
			if($info){
				if(is_administrator()){
	    			$agents = $user_model->all_agents();
	    			$this->assign("is_admin", 1);
	    			$this->assign("agents", $agents);
	    		}else{
	    			$this->assign("is_admin", 0);
	    		}
				$this->assign('info', $info);
				$this->meta_title = '修改普通';
            	$this->display();
			}else{
				$this->error('获取用户信息错误');
			}
    	}
    }
    
    /**
	 * 删除用户
	 */
	public function del(){
		$id = I('request.id', 0);
        if ( empty($id) ) {
            $this->error('请选择要操作的数据!');
        }
        if($id == C("USER_ADMINISTRATOR") || in_array(C("USER_ADMINISTRATOR"), $id)){
        	$this->error('超级管理员不可删除!');
        }
		$user_model = D('User');
		$id = array_unique((array)$id);
        $map = array('uid' => array('in', $id) );
        $t_map = $map;
        $t_map['type'] = 1;
        $temps = $user_model->where($t_map)->count();
        if($temps == 0){
        	if($user_model->where($map)->delete()){
        		D("Role")->unbind_relation($id);
	            $this->success('删除成功');
	        } else {
	            $this->error('删除失败！');
	        }
        }else{
        	$this->error('系统错误，删除失败！');
        }
	}
	
	/**
	 * 删除管理员
	 */
	public function del_root(){
		$id = I('request.id', 0);
        if ( empty($id) ) {
            $this->error('请选择要操作的数据!');
        }
        if($id == C("USER_ADMINISTRATOR") || in_array(C("USER_ADMINISTRATOR"), $id)){
        	$this->error('超级管理员不可删除!');
        }
		$user_model = D('User');
		$id = array_unique((array)$id);
        $map = array('uid' => array('in', $id) );
        $t_map = $map;
        $t_map['type'] = 1;
        $temps = $user_model->where($t_map)->count();
        if($temps == 0){
        	if($user_model->where($map)->delete()){
        		D("Role")->unbind_relation($id);
	            $this->success('删除成功');
	        } else {
	            $this->error('删除失败！');
	        }
        }else{
        	$this->error('系统错误，删除失败！');
        }
	}
	
	/**
	 * 删除代理商
	 */
	public function del_agent(){
		$id = I('request.id', 0);
        if ( empty($id) ) {
            $this->error('请选择要操作的数据!');
        }
        if($id == C("USER_ADMINISTRATOR") || in_array(C("USER_ADMINISTRATOR"), $id)){
        	$this->error('超级管理员不可删除!');
        }
		$user_model = D('User');
		$id = array_unique((array)$id);
        $map = array('uid' => array('in', $id) );
        $t_map = $map;
        $t_map['type'] = array("NEQ", 1);
        $temps = $user_model->where($t_map)->count();
        if($temps == 0){
        	$map2 = array();
        	$map2['puid'] = array('in', $id);
        	$temp2 = $user_model->where($map2)->count();
        	if($temp2 == 0){
        		if($user_model->where($map)->setField("is_del", 1)){
		            $this->success('删除成功');
		        } else {
		            $this->error('删除失败！');
		        }
        	}else{
        		$this->error('代理商下存在用户，无法删除！');
        	}
        }else{
        	$this->error('系统错误，删除失败！');
        }
	}
    
    /**
	 * 更改状态
	 */
	public function status($id,$value = 0){
        $id    = is_array($id) ? implode(',',$id) : $id;
        $where = array('uid' => array('in', $id ));
        $msg   = array_merge( array( 'success'=>'操作成功！', 'error'=>'操作失败！', 'url'=>'' ,'ajax'=>IS_AJAX) , (array)$msg );
		$data  = array('status'=>$value);
		$user_model = D("User");
        if( $user_model->where($where)->save($data)!==false ) {
            $this->success($msg['success'],$msg['url'],$msg['ajax']);
        }else{
            $this->error($msg['error'],$msg['url'],$msg['ajax']);
        }
	}
	
	/**
	 * 更改状态
	 */
	public function agent_status($id,$value = 0){
        $id    = is_array($id) ? implode(',',$id) : $id;
        $where = array('uid' => array('in', $id ));
        $msg   = array_merge( array( 'success'=>'操作成功！', 'error'=>'操作失败！', 'url'=>'' ,'ajax'=>IS_AJAX) , (array)$msg );
		$data  = array('status'=>$value);
		$user_model = D("User");
        if( $user_model->where($where)->save($data)!==false ) {
            $this->success($msg['success'],$msg['url'],$msg['ajax']);
        }else{
            $this->error($msg['error'],$msg['url'],$msg['ajax']);
        }
	}
	
	/**
	 * 更改状态
	 */
	public function root_status($id,$value = 0){
        $id    = is_array($id) ? implode(',',$id) : $id;
        $where = array('uid' => array('in', $id ));
        $msg   = array_merge( array( 'success'=>'操作成功！', 'error'=>'操作失败！', 'url'=>'' ,'ajax'=>IS_AJAX) , (array)$msg );
		$data  = array('status'=>$value);
		$user_model = D("User");
        if( $user_model->where($where)->save($data)!==false ) {
            $this->success($msg['success'],$msg['url'],$msg['ajax']);
        }else{
            $this->error($msg['error'],$msg['url'],$msg['ajax']);
        }
	}
}