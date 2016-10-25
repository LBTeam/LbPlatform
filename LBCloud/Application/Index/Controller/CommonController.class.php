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
        
        if(strtoupper(CONTROLLER_NAME) != 'INDEX' && session('is_full') == 1){
        	$this->error("请完善个人信息", U('Index/Index/information'));
        }
        
        if(!$this->_check_access()){
        	$this->error("访问失败，权限拒绝！");
        	exit;
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
		
		if(D("User")->is_agent(ADMIN_UID)){
			$this->assign("header_is_agent", 1);
		}else{
			$this->assign("header_is_agent", 0);
		}
		
		$configs = D("Config")->configs();
		$this->assign("configs", $configs);
	}
	
	/**
	 * 获取左侧菜单列表
	 */
	protected function _get_system_menu(){
		$user_id = session(C('USER_AUTH_KEY'));
		$sess_key = md5("{$user_id}_menus");
		if(session($sess_key)){
			$menus = session($sess_key);
		}else{
			$treeService = new TreeService();
			$menus = D('Node')->get_all_menu_nodes();
			if(!is_administrator()){
				$role_model = D("Role");
				$access_model = D("Access");
				$role_id = $role_model->role_id_by_user($user_id);
				$access = $access_model->access_by_role($role_id);
				foreach($menus as $key => $val){
					if(!in_array($val['id'], $access)){
						unset($menus[$key]);
					}
				}
			}
			$menus = $treeService->create_menu($menus);
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
	
	/**
	 * 检查权限列表
	 */
	protected function _check_access(){
		/*if(is_administrator()){
			return true;
		}else{
			$controller = strtoupper(CONTROLLER_NAME);
			$action = strtoupper(ACTION_NAME);
			$exec_name = "{$controller}/{$action}";
			$no_auth = explode(',', C("NOT_AUTH_MODULE"));
			if(in_array($controller, $no_auth)){
				return true;
			}else{
				$access_list = $this->_get_access_list();
				if(in_array($exec_name, $access_list)){
					return true;
				}else{
					return false;
				}
			}
		}*/
		$controller = strtoupper(CONTROLLER_NAME);
		$action = strtoupper(ACTION_NAME);
		$exec_name = "{$controller}/{$action}";
		$no_auth = explode(',', C("NOT_AUTH_MODULE"));
		if(in_array($controller, $no_auth)){
			return true;
		}else{
			$access_list = $this->_get_access_list();
			if(in_array($exec_name, $access_list)){
				return true;
			}else{
				return false;
			}
		}
	}
	
	/**
	 * 登录用户所有权限
	 * @return array
	 */
	protected function _get_access_list(){
		if($_SESSION['_ACCESS_LIST']){
			$access_list = $_SESSION['_ACCESS_LIST'];
		}else{
			/*$node_model = D("Node");
			$role_model = D("Role");
			$access_model = D("Access");
			$user_id = session("user_auth.uid");
			$nodes = $node_model->get_all_nodes();
			$role_id = $role_model->role_id_by_user($user_id);
			$access = $access_model->access_by_role($role_id);
			foreach($nodes as $key => $val){
				if(!in_array($val['id'], $access)){
					unset($nodes[$key]);
				}
			}
			$treeService = new TreeService();
			$access_list = $treeService->create_access($nodes);
			$_SESSION['_ACCESS_LIST'] = $access_list;*/
			$node_model = D("Node");
			$nodes = $node_model->get_all_nodes();
			if(!is_administrator()){
				$role_model = D("Role");
				$access_model = D("Access");
				$user_id = session("user_auth.uid");
				$role_id = $role_model->role_id_by_user($user_id);
				$access = $access_model->access_by_role($role_id);
				foreach($nodes as $key => $val){
					if(!in_array($val['id'], $access)){
						unset($nodes[$key]);
					}
				}
			}
			$treeService = new TreeService();
			$access_list = $treeService->create_access($nodes);
			$_SESSION['_ACCESS_LIST'] = $access_list;
		}
		return $access_list;
	}
	
}
