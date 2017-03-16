CREATE TABLE `player_version` (
	`id` INT(11) NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(255) NOT NULL DEFAULT '' COMMENT '版本名',
	`url` VARCHAR(255) NOT NULL DEFAULT '' COMMENT '存储地址',
	`version` VARCHAR(255) NOT NULL DEFAULT '' COMMENT '版本号',
	`addtime` INT(11) NOT NULL DEFAULT '0' COMMENT '发布时间',
	PRIMARY KEY (`id`)
)
COMMENT='版本列表'
COLLATE='utf8_general_ci'
ENGINE=InnoDB;

ALTER TABLE `player_player`
	ADD COLUMN `version` VARCHAR(64) NULL DEFAULT '' COMMENT '播放端版本号' AFTER `mac`;
	
添加节点
    系统管理
        版本管理 version 2
    系统管理>版本管理
        发布更新包 add_pack 1
        修改更新包 edit_pack 2
        删除更新包 del_pack 3
    屏幕管理>屏幕列表
        版本更新 version 17
        升级 ajax_do_upgrade 18
	
Modify file list path:
    LBCloud/Application/Api/Controller/DemoController.class.php
    LBCloud/Application/Api/Controller/PlayerController.class.php
    LBCloud/Application/Api/Model/CommandModel.class.php
    LBCloud/Application/Api/Service/AliyunOSS.class.php
    LBCloud/Application/Common/Conf/config.php
    LBCloud/Application/Index/Controller/IndexController.class.php
    LBCloud/Application/Index/Controller/ScreenController.class.php
    LBCloud/Application/Index/Controller/SystemController.class.php
    LBCloud/Application/Index/View/Record/index.html
    
Add file list path:
    LBCloud/Application/Index/Model/VersionModel.class.php
    LBCloud/Application/Index/View/System/add_pack.html
    LBCloud/Application/Index/View/System/list.html
    LBCloud/Public/Admin/oss/*
