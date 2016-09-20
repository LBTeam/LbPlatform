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
				$user_id = $user_model->add($data);
				if($user_id){
					$this->success('新增成功', U('root_list'));
				}else{
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
    
    /**
	 * 删除管理员和用户
	 */
	public function del(){
		$id = I('request.id', 0);
        if ( empty($id) ) {
            $this->error('请选择要操作的数据!');
        }
        if($id)
		$user_model = D('User');
		$id = array_unique((array)$id);
        $map = array('uid' => array('in', $id) );
        if($user_model->where($map)->delete()){
            $this->success('删除成功');
        } else {
            $this->error('删除失败！');
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
}