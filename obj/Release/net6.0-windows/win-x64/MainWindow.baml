   M S B A M L   `   `   `  ���� G  CKeyerProject, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:KeyerProjectKeyerProject      KeyerProject.MainWindow   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     ,localclr-namespace:KeyerProject       � Title$  Keyer��$	��760��$	��760��$
��1450��$
��1450��.���  � ColumnDefinitions �� $	��150 ��� $��1* �.+��� $��0��.+��� -    ��Name$ ImportButton��$��Загрузить��$	��100��$��24���� -   $ ExportButton��$��Сохранить��$	��100��$��24���� $��0,5,0,0q�$��Center=�$"��Выбранный цвет:���� 
���   �$��1q�$��5,5,5,5q�$	��100��$	��100��._��� -    ��Name$ ColorShowcase��$	��100��$	��100���� $��5,5,5,5q�
���   �$��1q�._��� $��	5,15,5,15q�.+��� $��Center=�$,��&Параметры обработки:��$��0,0,0,10q��� $��0,5,0,5q�.+��� $��0,0,0,5q�$&�� Влияние зелёного:���� -    ��Name$ k1Slider�� � Minimum$
 0.10Q� � Maximum$
 1.50Q� �� IsSnapToTickEnabled$
 True�� �� TickFrequency$
 0.05Q��� -   	 ��Name$	 k1Box��$��24��$��Center=����� 
 �� ElementName$
 k1Slider�� �� Path$ Value� �� StringFormat$ k1: {0:#0.00}���� -    ��Name$ AutoChooseK1Box��$��Автовыбор�� d� 	IsChecked �� *k1BoxChecked+$
 _uiThis���� -   $	 RecommendationText���� $��0,5,0,5q�.+��� $��0,0,0,5q�$"��Влияние синего:���� -   $ k2Slider��$
 0.10Q�$
 1.50Q�$
 True�� �� TickPlacement$
 Both=�$
 0.05Q��� -	   $	 k2Box��$��24��$��Center=����� $
 k2Slider��$ Value�$ k2: {0:#0.00}���� $��0,5,0,5q�.+��� $��0,0,0,5q�$&�� Влияние красного:���� -
   $ k3Slider��$
 0.10Q� � Value$ 1Q�$
 1.50Q�$
 True��$
 Both=�$
 0.05Q��� -   $	 k3Box��$��24��$��Center=����� $
 k3Slider��$ Value�$ k3: {0:#0.00}���� $��0,5,0,5q�.+��� $'��!Чувствительность:���� -   $ PrecisionSlider��$��5,5,5,5q�$ 5Q�$ 1Q�$	 100Q�$��24��$
 True��$ 5Q�$
 Both=��� -   $	 PrecisionBox��$��24��$��Center=����� $
 PrecisionSlider��$ Value�$
 {0}%���� $��0,5,0,5q�.+��� $,��&Прозрачность границ:���� -   $ 
BlurSlider��$��5,5,5,5q�$ 5Q�$ 30Q�$ 80Q�$��24��$
 True��$ 5Q�$
 Both=��� -   $	 BlurBox��$��24��$��Center=����� $
 
BlurSlider��$ Value�$
 {0}%���� -   $ ProcessButton��$��5,5,5,5q�$��Обработка��$	��100��$��24���� 
���   �$��1q�$��1��$��0,5,10,5q�._��� $��Visible=�.���� -    ��Name$ ImagePreview��$	��Top=�$
��Left=�$��9,9,0,0q� J� RenderTransformOrigin$ 0.496,0.518&�