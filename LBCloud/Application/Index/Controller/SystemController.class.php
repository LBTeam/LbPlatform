<?php
/**
 * 系统配置controller
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Controller;
use Api\Service\AliyunOSS;

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
                $model->rollback();
            	$this->error("保存失败！");
            }
        }else{
        	$this->error("参数错误，保存失败！");
        }
    }

    /**
     * 版本列表
     */
    public function version(){
        $model = D("Version");
        $_list = $model->all_packages();
        $this->meta_title = "版本列表";
        $this->assign("_list", $_list);
        $this->display("list");
    }

    /**
     * 发布版本
     */
    public function add_pack(){
        if(IS_POST){
            $model = D("Version");
            $rules = array(
                array('name','require','版本名称不能为空！'),
                array('version','require','版本号不能为空！'),
                array('package','require','请上传更新包！')
            );
            if($model->validate($rules)->create()){
                $bucket = C("oss_version_bucket");
                $endponit = C("aliyun_oss_endpoint");
                $array = parse_url($endponit);
                $host = "{$array['scheme']}://{$bucket}.{$array['host']}/";

                $data = array();
                $data['name'] = I("post.name");
                $data['url'] = $host.I("post.package");
                $data['version'] = I("post.version");
                $data['addtime'] = NOW_TIME;
                $id = $model->add($data);
                if($id){
                    $this->success('版本发布成功!', U('version'));
                }else{
                    $this->error('版本发布失败!');
                }
            }else{
                $this->error($model->getError());
            }
        }else{
            $this->meta_title = "发布更新包";
            $this->display("add_pack");
        }
    }

    /**
     * 删除更新包
     */
    public function del_pack(){
        $id = I('request.id', 0);
        if ( empty($id) ) {
            $this->error('请选择要操作的数据!');
        }
        $model = D('Version');
        $id = array_unique((array)$id);
        $map = array('id' => array('in', $id) );
        $packages = $model->package_by_ids($id, "url");
        if($model->where($map)->delete()){
            $objects = array();
            foreach ($packages as $val){
                $temp = explode('/', $val['url']);
                $objects[] = end($temp);
            }
            if($objects){
                $obj = new AliyunOSS();
                $bucket = C("oss_version_bucket");
                $obj->delete_objects($objects, $bucket);
            }
            $this->success('删除成功！');
        } else {
            $this->error('删除失败！');
        }
    }
}
