<?php
/**
 * 播放端版本model
 * @author liangjian
 * @email 15934854815@163.com
 */

namespace Api\Model;
use Think\Model;

class VersionModel extends Model
{
    /**
     * 最新的更新包
     * @return array
     */
    public function last_package(){
        return $this->order("id DESC")->find();
    }
}