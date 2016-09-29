<?php
/**
 * 系统配置controller
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Controller;

class SystemController extends CommonController
{
	private $config_type;
	public function _initialize(){
		parent::_initialize();
		$this->config_type = array(
			'1'	=>	'基本配置',
			'2'	=>	'开发配置',
		);
	}
	
	public function index(){
		$id = I('get.id',1);
		$template = "index_{$id}";
		$cfg_model = D("Config");
		$cfgs = $cfg_model->configs();
		$this->assign("cfgs", $cfgs);
		$this->meta_title = $this->config_type[$id];
		$this->display($template);
	}
	
	public function save($config){
        if($config && is_array($config)){
            $cfg_model = D("Config");
            $cfg_param = array();
            foreach ($config as $key => $value) {
                $map = array('key' => $key);
                $cfg = $cfg_model->where($map)->setField('value', $value);
                if(!$cfg){
                	$cfg_param[] = array(
                		'key' => $key,
                		'value'	=> $value
                	);
                }
            }
            if($cfg_param){
            	$cfg_model->addAll($cfg_param);
            }
        }
        $this->success('保存成功！');
    }
}
