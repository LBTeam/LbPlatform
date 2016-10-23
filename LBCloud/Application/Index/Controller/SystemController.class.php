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
			'3'	=>	'文件清理配置',
			'4'	=>	'监控图片清理配置'
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
            $cfg_keys = array_keys($config);
            $map = array('key' => array('IN', $cfg_keys));
            foreach ($config as $key => $value) {
            	$cfg_param[] = array(
            		'key' => $key,
            		'value'	=> $value
            	);
            }
            $model = new \Think\Model();
            $model->startTrans();
            $del_res = $cfg_model->where($map)->delete();
            $add_res = true;
            if($cfg_param){
            	$add_res = $cfg_model->addAll($cfg_param);
            }
            if($del_res !== false && $add_res){
            	$model->commit();
            	$this->success('保存成功！');
            }else{
            	$modle->rollback();
            	$this->error("保存失败！");
            }
        }else{
        	$this->error("参数错误，保存失败！");
        }
    }
}
