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
				array('name','require','用户组不能为空！'),
				array('name','','用户组已经存在！',0,'unique',1)
			);
			$data = array();
			$data['name'] = I("post.name", "");
			$data['status'] = I("post.status", 0);
			$data['pid'] = 0;
			$data['remark'] = I("post.remark", "");
			if($role_model->validate($rules)->create($data)){
				$id = $role_model->add();
				if($id){
                    $this->success('新增成功', U('index'));
                } else {
                    $this->error('新增失败');
                }
			}else{
				$this->error($role_model->getError());
			}
		}else{
			$this->meta_title = '新增管理员';
            $this->display();
		}
	}
	
	/**
	 * 用户列表
	 */
    public function index(){
    	$user_model = D("User");
    	$users = $user_model->all_user_list();
    	$condition = array(
    		"status" => array(
    			0 => "正常",
    			1 => "禁用"
    		),
    		"type" => array(
    			0 => "管理员",
    			1 => "代理用户",
    			2 => "普通用户"
    		)
    	);
    	int_to_string($users, $condition);
    	$this->assign("users", $users);
    	$this->meta_title = '用户列表';
		$this->display();
    }
    
    /**
     * 添加用户
     */
    public function add(){
    	if(IS_POST){
    		
    	}else{
    		$this->meta_title = '新增用户组';
            $this->display();
    	}
    }
}