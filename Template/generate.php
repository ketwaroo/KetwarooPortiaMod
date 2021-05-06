<?php

// using php to automate this because getting powershell to run scripts is a pain in the ass.

require_once 'ruut.framework.php';

use Ketwaroo\Xml;
use Ketwaroo\FileSystem;

if (isset($argv[1])) {
    $csproj = [$argv[1]];
}
else {
    $csproj = FileSystem::readFilesInDirectoryGlob(dirname(__DIR__), '*.csproj');
}

$targetDir   = FileSystem::osPath('E:/GAMES/My Time At Portia/Mods');
$defaultInfo = json_decode(file_get_contents(__DIR__ . '/InfoTpl.json'));

foreach ($csproj as $c) {
    $c            = FileSystem::osPath($c);
    $x            = Xml::loadXmlFile($c);
    // in case assembly name is different.
    $assemblyName = strval($x->PropertyGroup[0]->AssemblyName);

    if (empty($assemblyName)) {
        continue;
    }

    $dllPath = FileSystem::osPath(dirname($c) . '/bin/Release/' . $assemblyName . '.dll');
    if (!is_file($dllPath)) {
        continue;
    }
    $outDir      = FileSystem::osPath("{$targetDir}/$assemblyName");
    $outInfoJson = FileSystem::osPath("{$outDir}/Info.json");
    $outDll      = FileSystem::osPath("{$outDir}/{$assemblyName}.dll");

    if (is_file($outDll) && filemtime($outDll) >= filemtime($dllPath)) {
        prnt("skip $assemblyName");
        continue;
    }
    prnt("deploy $assemblyName");

    $info = is_file($outInfoJson) ? json_decode(file_get_contents($outInfoJson)) : $defaultInfo;
    if (empty($info)) {
        $info = clone $defaultInfo;
    }

    $info->Id          = $assemblyName;
    $info->Author      = 'ketwaroo';
    $info->Version     = '0.1.' . filemtime($dllPath);
    // eh. whatever
    $info->DisplayName = $assemblyName;

    $info->AssemblyName = "{$assemblyName}.dll";
    $info->EntryMethod  = "{$assemblyName}.Main.Load";

    FileSystem::prepareDirectory($outDir);

    copy($dllPath, $outDll);
    file_put_contents($outInfoJson, json_encode($info, JSON_PRETTY_PRINT));

    $z = new ZipArchive();

    $z->open(__DIR__ . "/{$assemblyName}.zip", ZipArchive::CREATE | ZipArchive::OVERWRITE);

    $z->addFile($outDll, basename($outDll));
    $z->addFile($outInfoJson, basename($outInfoJson));
    $z->close();
}

