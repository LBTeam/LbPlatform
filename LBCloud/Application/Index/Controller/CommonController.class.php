<?php
/**
 * 基类controller
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Controller;
use Think\Controller;
use Index\Service\TreeService;

class CommonController extends Controller
{
	protected function _initialize(){
		define('ADMIN_UID', is_login());
		if(!ADMIN_UID){// 还没登录 跳转到登录页面
            $this->redirect('Public/login');
        }
		
		$menus = $this->_get_system_menu();
		foreach($menus as &$val){
			$temp = explode('/', $val['name']);
			if(strtoupper(CONTROLLER_NAME) == strtoupper($temp[1])){
				$val['display'] = 1;
			}else{
				$val['display'] = 0;
			}
		}
		$this->assign('menus', $menus);
	}
	
	protected function _get_system_menu(){
		$user_id = session(C('USER_AUTH_KEY'));
		$sess_key = md5("{$user_id}_menus");
		if(session($sess_key)){
			$menus = session($sess_key);
		}else{
			$menus = D('Node')->get_all_menu_nodes();
			if(!is_administrator()){
				/*$roles_id = D('User')->get_roles_by_user_id($user_id);
				$access = D('Access')->get_access_by_roles_id($roles_id);
				foreach($menu as $key => $val){
					if(!in_array($val['id'], $access)){
						unset($menu[$key]);
					}
				}*/
			}
			foreach($menus as &$val){
				$val['name'] = MODULE_NAME."/{$val['name']}/";
				foreach($val['children'] as &$v){
					$v['name'] = "{$val['name']}{$v['name']}";
				}
			}
			session($sess_key, $menus);
		}
		return $menus;
	}
}
