pushd
cd /d %0\.. 

NServiceBus.Host.exe /uninstall /serviceName:TPTaskCreator
Tp.Integration.Plugin.UninstallUtil.exe Tp.TaskCreator Tp.PubSubStorage

popd
exit 0