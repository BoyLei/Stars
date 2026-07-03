config=./npm.cfg
# 从外部配置文件中读取NPM信息
npmServer=`sed '/^server=/!d;s/.*=//' $config`  #字符串的变量npmServer， 这个值是公司内部搭建的npm私有服务器的ip地址
user=`sed '/^user=/!d;s/.*=//' $config` #字符串的变量user，这是每个开发者的账号，自己填自己的
echo npmServer=$npmServer
echo user=$user

# 将npm的当前登录的用户名赋值到loggedUser变量，npm whoami --registry $npmServer原意是将npmServer的当前用户名打印到控制台，$()这个bash语法将一些括号内执行的操作结果获得，然后用其外部的命令对这些结果进行操作，这里就是将输出的结果用等号赋值给loggedUser变量
loggedUser=$(npm $user --registry $npmServer)

# 在npmServer指向的网址发布UPKHelloWorld路径指定的包 
npm publish --registry $npmServer