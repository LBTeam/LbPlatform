<?php
return array(
	//'配置项'=>'配置值'
	'USER_ADMINISTRATOR'		=>	1,		//超级管理员ID
	'USER_AUTH_ON'              =>  true,
	'USER_AUTH_TYPE'			=>  1,		// 默认认证类型 1 登录认证 2 实时认证
	'USER_AUTH_KEY'             =>  'UID',	// 用户认证SESSION标记
	'ADMIN_AUTH_KEY'			=>	'ADMIN_UID',
	'NOT_AUTH_MODULE'			=>	'PUBLIC,INDEX',
	
	'USER_AUTH_MODEL'           =>  'User',	// 默认验证数据表模型
	'USER_AUTH_GATEWAY'         =>  '',// 默认认证网关
	//'NOT_AUTH_MODULE'           =>  '',		// 默认无需认证模块
	'REQUIRE_AUTH_MODULE'       =>  '',		// 默认需要认证模块
	'NOT_AUTH_ACTION'           =>  '',		// 默认无需认证操作
	'REQUIRE_AUTH_ACTION'       =>  '',		// 默认需要认证操作
	'RBAC_ROLE_TABLE'           =>  'player_role',
	'RBAC_USER_TABLE'           =>  'player_role_user',
	'RBAC_ACCESS_TABLE'         =>  'player_access',
	'RBAC_NODE_TABLE'           =>  'player_node',
	
	'VERIFY_ENABLE' 	=> false,
	'ROOT_ROLE_ID'		=> 1,
	'AGENT_ROLE_ID'		=> 2,
	'NORMAL_ROLE_ID'	=> 3,
	'DATA_SIGN_KEY'		=> 'J6zAkThc',
	/*发送验证码*/
   	"SMS_SERVER"	=>	"http://msg.ddt123.cn/api.php/Api/sendmsg/sendMobMsg?",
   	"EMAIL_SERVER"	=>	"http://123.56.240.172:8084/index.php?m=home&c=index&a=Mailto&",
	
	/* 模板相关配置 */
    'TMPL_PARSE_STRING' => array(
        '__STATIC__' => __ROOT__ . '/Public/static',
        '__IMG__'    => __ROOT__ . '/Public/' . MODULE_NAME . '/images',
        '__CSS__'    => __ROOT__ . '/Public/' . MODULE_NAME . '/css',
        '__JS__'     => __ROOT__ . '/Public/' . MODULE_NAME . '/js',
    ),
    
	/* 后台错误页面模板 */
    'TMPL_ACTION_ERROR'     =>  MODULE_PATH.'View/Public/error.html', // 默认错误跳转对应的模板文件
    'TMPL_ACTION_SUCCESS'   =>  MODULE_PATH.'View/Public/success.html', // 默认成功跳转对应的模板文件
    'TMPL_EXCEPTION_FILE'   =>  MODULE_PATH.'View/Public/exception.html',// 异常页面的模板文件
);