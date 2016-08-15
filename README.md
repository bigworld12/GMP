#Official site :
  https://bigworld12.github.io/GMP/
#How can I contribute ?
  1. When you copy the project data into visual studio it will fail to compile because of the updater code that I removed 
     specifically here  : https://github.com/bigworld12/GMP/blob/989ff31f9fd5e74ea8d71ffd980bebef5d9ba760/GMP.sln#L10
     And here : https://github.com/bigworld12/GMP/blob/989ff31f9fd5e74ea8d71ffd980bebef5d9ba760/GMP/MainWindow.xaml.cs#L68
  2. So what to do ? Simply remove the project reference to the shared project "ProtectionManager"
  3. Comment out the updater code in here https://github.com/bigworld12/GMP/blob/989ff31f9fd5e74ea8d71ffd980bebef5d9ba760/GMP/MainWindow.xaml.cs#L68
  4. Then you are good to go ^^
