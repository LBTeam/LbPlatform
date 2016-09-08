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
                    case -1: $error = '用户不存在！'; break; //系统级别禁用
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
            $this->success('退出成功！', U('login'));
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
}
