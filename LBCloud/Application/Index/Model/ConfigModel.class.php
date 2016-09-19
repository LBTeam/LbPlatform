<?php
/**
 * 系统配置model
 * @author liangjian
 * @email 159348548115@163.com
 */
namespace Index\Model;
use Think\Model;

class ConfigModel extends Model{
	public function configs(){
		return $this->getField("key,value");
	}
	
	
	public function roles(){
		$roles = array();
		$cfgs = $this->getField("key,value");
		$roles['root'] = $cfgs['ROOT_ROLE_ID'] ? $cfgs['ROOT_ROLE_ID'] : C("ROOT_ROLE_ID");
		$roles['agent'] = $cfgs['AGENT_ROLE_ID'] ? $cfgs['AGENT_ROLE_ID'] : C("AGENT_ROLE_ID");
		$roles['normal'] = $cfgs['NORMAL_ROLE_ID'] ? $cfgs['NORMAL_ROLE_ID'] : C("NORMAL_ROLE_ID");
		return $roles;
	}
}
