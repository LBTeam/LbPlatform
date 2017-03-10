<?php
return array(
	//'配置项'=>'配置值'
	'MODULE_ALLOW_LIST'	=>	array('Api','Home','Index', 'Admin'),
	'MODULE_DENY_LIST'	=>	array('Common','Runtime'),
	'DEFAULT_MODULE'	=>	'Index',
	//数据库配置
	'DB_TYPE'		=>  'mysql',		// 数据库类型
	'DB_HOST'		=>  '127.0.0.1',	// 服务器地址
	'DB_NAME'		=>  'player',		// 数据库名
	'DB_USER'		=>  'root',			// 用户名
	'DB_PWD'		=>  '',				// 密码
	//'DB_USER'		=>	'root',      	// 用户名
	//'DB_PWD'		=>	'lkgx',			// 密码
	'DB_PORT'		=>  '3306',			// 端口
	'DB_PREFIX'		=>  'player_',		// 数据库表前缀
    
    'pwd_auth_key'	=>	'yFjXp2Qxke',	//用户密码加密密钥
    
    //oss配置
    'aliyun_oss_id'			=>	'f1mcwCSSqB9tIY57',
	'aliyun_oss_secret'		=>	'7aSeEDlyyV5pIddw49FhLXFMVvG9UK',
	'aliyun_oss_endpoint'	=>	'http://oss-cn-shenzhen.aliyuncs.com',
	'aliyun_oss_bucket'		=>	'lb-player-test',
	'oss_media_bucket'		=>	'lb-player-media',
	'oss_program_bucket'	=>	'lb-player-program',
	'oss_picture_bucket'	=>	'lb-monitor-picture',
    'oss_version_bucket'    =>  'lb-software-library',
	
	//相关服务器配置
	'redis_server'		=>	'10.171.126.247',
	'redis_port'		=>	6379,
	'websocket_ip'		=>	'123.56.240.172',
	'websocket_port'	=>	'9501',
);