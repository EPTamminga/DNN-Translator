﻿Imports System.ComponentModel

Namespace ViewModel
 Public Class ResourceCollectionViewModel
  Inherits WorkspaceViewModel

#Region " Constructors "
  Public Sub New(mainWindow As MainWindowViewModel)
   Me.MainWindow = mainWindow
   Me.TargetLocale = mainWindow.SelectedLocale
   Me.TargetColumnHeader = mainWindow.SelectedLocale.Name
  End Sub
#End Region

#Region " Properties "
  Public Property Exists As Boolean = True
  Public Property HasSelection As Boolean = False
  Public Property HasBingLocale As Boolean = False
  Public Property MainWindow As MainWindowViewModel = Nothing

  Private _HasChanges As Boolean = False
  Public Property HasChanges() As Boolean
   Get
    Return _HasChanges
   End Get
   Set(ByVal value As Boolean)
    _HasChanges = value
    If value Then
     If Not DisplayName.EndsWith(" *") Then DisplayName &= " *"
    Else
     If DisplayName.EndsWith(" *") Then DisplayName = DisplayName.Substring(0, DisplayName.Length - 2)
    End If
    Me.OnPropertyChanged("HasChanges")
   End Set
  End Property

  Private _TargetColumnHeader As String = ""
  Public Property TargetColumnHeader() As String
   Get
    Return _TargetColumnHeader
   End Get
   Set(ByVal value As String)
    _TargetColumnHeader = value
    Me.OnPropertyChanged("TargetColumnHeader")
   End Set
  End Property

  Private _CompareColumnHeader As String = "Temporary Workspace"
  Public Property CompareColumnHeader() As String
   Get
    Return _CompareColumnHeader
   End Get
   Set(ByVal value As String)
    _CompareColumnHeader = value
    Me.OnPropertyChanged("CompareColumnHeader")
   End Set
  End Property

  Private _ShowFileColumn As Boolean = False
  Public Property ShowFileColumn() As Boolean
   Get
    Return _ShowFileColumn
   End Get
   Set(ByVal value As Boolean)
    _ShowFileColumn = value
    Me.OnPropertyChanged("ShowFileColumn")
   End Set
  End Property

  Private _ShowCompareColumn As Boolean = False
  Public Property ShowCompareColumn() As Boolean
   Get
    Return _ShowCompareColumn
   End Get
   Set(ByVal value As Boolean)
    _ShowCompareColumn = value
    Me.OnPropertyChanged("ShowCompareColumn")
   End Set
  End Property

  Private _ShowTargetColumn As Boolean = True
  Public Property ShowTargetColumn() As Boolean
   Get
    Return _ShowTargetColumn
   End Get
   Set(ByVal value As Boolean)
    _ShowTargetColumn = value
    Me.OnPropertyChanged("ShowTargetColumn")
   End Set
  End Property

  Public ReadOnly Property IsResourceCollection As Boolean
   Get
    Return True
   End Get
  End Property
  Public Property IsRemoteResourceCollection As Boolean = False

  Private _bingLocale As CultureInfo
  Public Property BingLocale() As CultureInfo
   Get
    Return _bingLocale
   End Get
   Set(ByVal value As CultureInfo)
    _bingLocale = value
    Me.OnPropertyChanged("BingLocale")
    HasBingLocale = CBool(value IsNot Nothing)
    Me.OnPropertyChanged("HasBingLocale")
   End Set
  End Property

  Private _targetLocale As CultureInfo = Nothing
  Public Property TargetLocale() As CultureInfo
   Get
    Return _targetLocale
   End Get
   Set(ByVal value As CultureInfo)
    _targetLocale = value
    Me.OnPropertyChanged("TargetLocale")
    Dim ts As New Common.TranslatorSettings
    BingLocale = ts.BingLocales.FirstOrDefault(Function(x) x.Name = _targetLocale.Name)
    If BingLocale Is Nothing Then
     BingLocale = ts.BingLocales.FirstOrDefault(Function(x) x.Name = _targetLocale.Parent.Name)
    End If
   End Set
  End Property

  Private _ResourceKeys As New Common.SortableObservableCollection(Of ResourceKeyViewModel)
  Public ReadOnly Property ResourceKeys As Common.SortableObservableCollection(Of ResourceKeyViewModel)
   Get
    Return _ResourceKeys
   End Get
  End Property

  Public Sub AddKeys(keys As IEnumerable(Of ResourceKeyViewModel))
   For Each k As ResourceKeyViewModel In keys
    _ResourceKeys.Add(k)
    AddHandler k.PropertyChanged, AddressOf ResourceChanged
   Next
  End Sub
  Public Sub AddKey(key As ResourceKeyViewModel)
   _ResourceKeys.Add(key)
   AddHandler key.PropertyChanged, AddressOf ResourceChanged
  End Sub

  Private _selectedList As List(Of ResourceKeyViewModel) = Nothing
  Public Property SelectedList() As List(Of ResourceKeyViewModel)
   Get
    If _selectedList Is Nothing Then
     _selectedList = New List(Of ResourceKeyViewModel)
     For Each rk As ResourceKeyViewModel In ResourceKeys
      If rk.Selected Then
       _selectedList.Add(rk)
      End If
     Next
    End If
    Return _selectedList
   End Get
   Set(ByVal value As List(Of ResourceKeyViewModel))
    _selectedList = value
   End Set
  End Property
#End Region

#Region " Select All "
  Private _selectAllCommand As RelayCommand
  Public ReadOnly Property SelectAllCommand As RelayCommand
   Get
    If _selectAllCommand Is Nothing Then
     _selectAllCommand = New RelayCommand(Sub(param) SelectAllCommandHandler())
    End If
    Return _selectAllCommand
   End Get
  End Property

  Public Sub SelectAllCommandHandler()
   For Each rk As ResourceKeyViewModel In ResourceKeys
    rk.Selected = True
   Next
  End Sub
#End Region

#Region " Copying "
  Private _copyAllCommand As RelayCommand
  Public ReadOnly Property CopyAllCommand As RelayCommand
   Get
    If _copyAllCommand Is Nothing Then
     _copyAllCommand = New RelayCommand(Sub(param) CopyAllCommandHandler(param))
    End If
    Return _copyAllCommand
   End Get
  End Property

  Public Sub CopyAllCommandHandler(param As Object)
   Dim copyParams As String = CType(param, String)
   Select Case copyParams
    Case "All,Original2Target"
     For Each rkv As ResourceKeyViewModel In ResourceKeys
      rkv.TargetValue = rkv.OriginalValue
     Next
    Case "Empty,Original2Target"
     For Each rkv As ResourceKeyViewModel In ResourceKeys
      If String.IsNullOrEmpty(rkv.TargetValue) Then
       rkv.TargetValue = rkv.OriginalValue
      End If
     Next
    Case "Selected,Original2Target"
     For Each rkv As ResourceKeyViewModel In ResourceKeys
      If rkv.Selected Then
       rkv.TargetValue = rkv.OriginalValue
      End If
     Next
    Case "All,Compare2Target"
     For Each rkv As ResourceKeyViewModel In ResourceKeys
      rkv.TargetValue = rkv.CompareValue
     Next
    Case "Empty,Compare2Target"
     For Each rkv As ResourceKeyViewModel In ResourceKeys
      If String.IsNullOrEmpty(rkv.TargetValue) Then
       rkv.TargetValue = rkv.CompareValue
      End If
     Next
    Case "Selected,Compare2Target"
     For Each rkv As ResourceKeyViewModel In ResourceKeys
      If rkv.Selected Then
       rkv.TargetValue = rkv.CompareValue
      End If
     Next
    Case "All,Original2Compare"
     For Each rkv As ResourceKeyViewModel In ResourceKeys
      rkv.CompareValue = rkv.OriginalValue
     Next
     ShowCompareColumn = True
    Case "Empty,Original2Compare"
     For Each rkv As ResourceKeyViewModel In ResourceKeys
      If String.IsNullOrEmpty(rkv.TargetValue) Then
       rkv.CompareValue = rkv.OriginalValue
      End If
     Next
     ShowCompareColumn = True
    Case "Selected,Original2Compare"
     For Each rkv As ResourceKeyViewModel In ResourceKeys
      If rkv.Selected Then
       rkv.CompareValue = rkv.OriginalValue
      End If
     Next
     ShowCompareColumn = True
   End Select
  End Sub
#End Region

#Region " Bing "
  Private _bingTranslateCommand As RelayCommand
  Public ReadOnly Property BingTranslateCommand As RelayCommand
   Get
    If _bingTranslateCommand Is Nothing Then
     _bingTranslateCommand = New RelayCommand(Sub(param) BingTranslateCommandHandler(param))
    End If
    Return _bingTranslateCommand
   End Get
  End Property

  Public Sub BingTranslateCommandHandler(param As Object)
   If BingLocale Is Nothing Then Exit Sub
   Dim translateParams As String = CType(param, String)
   Dim toTranslate As New Dictionary(Of String, String)
   If translateParams.StartsWith("All") Then
    For Each rkv As ResourceKeyViewModel In ResourceKeys
     toTranslate.Add(rkv.Key, rkv.OriginalValue)
    Next
   ElseIf translateParams.StartsWith("Empty") Then
    For Each rkv As ResourceKeyViewModel In ResourceKeys
     If String.IsNullOrEmpty(rkv.TargetValue) Then
      toTranslate.Add(rkv.Key, rkv.OriginalValue)
     End If
    Next
   ElseIf translateParams.StartsWith("Selected") Then
    For Each rkv As ResourceKeyViewModel In ResourceKeys
     If rkv.Selected Then
      toTranslate.Add(rkv.Key, rkv.OriginalValue)
     End If
    Next
   End If
   Dim translated As Dictionary(Of String, String) = MainWindow.Bing.Translate(toTranslate, BingLocale)
   If translateParams.EndsWith("2Target") Then
    For Each rkv As ResourceKeyViewModel In ResourceKeys
     If translated.ContainsKey(rkv.Key) Then
      rkv.TargetValue = translated(rkv.Key)
     End If
    Next
   ElseIf translateParams.EndsWith("2Compare") Then
    For Each rkv As ResourceKeyViewModel In ResourceKeys
     If translated.ContainsKey(rkv.Key) Then
      rkv.CompareValue = translated(rkv.Key)
     End If
    Next
    ShowCompareColumn = True
   End If
  End Sub
#End Region

#Region " Resource Changed Handler "
  Friend Overridable Sub ResourceChanged(sender As Object, e As PropertyChangedEventArgs)
   Select Case e.PropertyName
    Case "Selected"
     _selectedList = Nothing ' reset the selection list
     HasSelection = True
     Me.OnPropertyChanged("HasSelection")
    Case "TargetValue"
     HasChanges = True
   End Select
  End Sub
#End Region

#Region " Html Edit "
  Private _editHtmlCommand As RelayCommand
  Public ReadOnly Property EditHtmlCommand As RelayCommand
   Get
    If _editHtmlCommand Is Nothing Then
     _editHtmlCommand = New RelayCommand(Sub(param) EditHtmlCommandHandler(param))
    End If
    Return _editHtmlCommand
   End Get
  End Property

  Protected Sub EditHtmlCommandHandler(param As Object)
   For Each rkv As ResourceKeyViewModel In ResourceKeys
    If rkv.Selected Then
     Dim k As String = rkv.ID
     Dim ws As WorkspaceViewModel = CType(MainWindow.Workspaces.FirstOrDefault(Function(vm) vm.ID = k), HtmlKeyViewModel)
     If ws Is Nothing Then
      ws = New HtmlKeyViewModel(MainWindow, rkv, TargetLocale)
      MainWindow.Workspaces.Add(ws)
     End If
     MainWindow.SetActiveWorkspace(ws)
    End If
   Next
  End Sub
#End Region

#Region " Closing "
  Protected Overrides Sub OnRequestClose()
   If HasChanges AndAlso TargetLocale IsNot Nothing Then
    Dim messageBoxText As String = "Do you want to save changes?"
    Dim caption As String = "Word Processor"
    Dim button As MessageBoxButton = MessageBoxButton.YesNoCancel
    Dim icon As MessageBoxImage = MessageBoxImage.Warning
    Dim result As MessageBoxResult = MessageBox.Show(messageBoxText, caption, button, icon)
    Select Case result
     Case MessageBoxResult.Yes
      Save()
     Case MessageBoxResult.No
      ' continue closing
     Case MessageBoxResult.Cancel
      Exit Sub
    End Select
   End If
   CloseMe()
  End Sub

  Public Sub Save()
   ' get a list of files
   Dim fileList As New List(Of String)
   For Each k As ResourceKeyViewModel In From x In ResourceKeys Where x.Changed Select x
    If Not fileList.Contains(k.ResourceFile) Then
     fileList.Add(k.ResourceFile)
    End If
   Next
   ' handle each file
   For Each resFile As String In fileList
    Dim fileKey As String = resFile
    Dim targetFile As String = MainWindow.ProjectSettings.Location & "\" & resFile.Replace(".resx", "." & TargetLocale.Name & ".resx")
    Dim targetResx As New Common.ResourceFile(fileKey, targetFile)
    For Each k As ResourceKeyViewModel In From x In ResourceKeys Where x.Changed And x.ResourceFile = fileKey And Trim(x.TargetValue) <> "" Select x
     targetResx.SetResourceValue(k.Key, k.TargetValue)
     k.Changed = False
    Next
    targetResx.Save()
   Next
   HasChanges = False
  End Sub
#End Region

#Region " Saving "
  Private _SaveCommand As RelayCommand
  Public ReadOnly Property SaveCommand As RelayCommand
   Get
    If _SaveCommand Is Nothing Then
     _SaveCommand = New RelayCommand(Sub(param)
                                      Save()
                                     End Sub,
                                     Function()
                                      Return HasChanges
                                     End Function)
    End If
    Return _SaveCommand
   End Get
  End Property
#End Region

#Region " Uploading "
  Private _UploadCommand As RelayCommand
  Public ReadOnly Property UploadCommand As RelayCommand
   Get
    If _UploadCommand Is Nothing Then
     _UploadCommand = New RelayCommand(Sub(param) UploadCommandHandler(param))
    End If
    Return _UploadCommand
   End Get
  End Property

  Public Sub UploadCommandHandler(param As Object)
   Save() ' we save the changes
   Dim what As String = CType(param, String).ToLower
   Dim tosend As New List(Of Common.LEService.TextInfo)
   Select Case what
    Case "all"
     For Each rkv As ResourceKeyViewModel In ResourceKeys
      If rkv.LEText IsNot Nothing Then
       rkv.LEText.Locale = MainWindow.ProjectSettings.MappedLocale
       rkv.LEText.Translation = rkv.TargetValue
       tosend.Add(rkv.LEText)
      End If
     Next
    Case "selected"
     For Each rkv As ResourceKeyViewModel In ResourceKeys
      If rkv.Selected Then
       If rkv.LEText IsNot Nothing Then
        rkv.LEText.Locale = MainWindow.ProjectSettings.MappedLocale
        rkv.LEText.Translation = rkv.TargetValue
        tosend.Add(rkv.LEText)
       End If
      End If
     Next
   End Select
   Dim service As New Common.LEService.LEService(MainWindow.ProjectSettings.ConnectionUrl, MainWindow.ProjectSettings.Username, MainWindow.ProjectSettings.Password)
   service.UploadResources(tosend)
  End Sub
#End Region

 End Class
End Namespace