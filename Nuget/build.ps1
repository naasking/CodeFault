$vers="1.0.0-RC1"
$files=@("CodeFault")

rm -R *.nupkg
foreach($i in $files)
{
  rmdir -R "$i"
  #mkdir "$i/lib/net35", "$i/lib/net40"
  #cp "../$i/bin/Release v3.5/$i.dll" "$i/lib/net35"
  #cp "../$i/bin/Release v3.5/$i.xml" "$i/lib/net35"
  mkdir "$i/lib/net40"
  cp "../$i/bin/Release/$i.dll" "$i/lib/net40"
  cp "../$i/bin/Release/$i.xml" "$i/lib/net40"
  cp "../LICENSE" "CodeFault"
  cat "CodeFault.nuspec.xml" | %{$_ -replace "[$]vers[$]", $vers } > CodeFault/CodeFault.nuspec
  ./nuget pack "$i/$i.nuspec"
}
