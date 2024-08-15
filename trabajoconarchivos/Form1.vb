Imports System.IO

Public Class Form1
    Private Sub TreeViewArchivos_BeforeExpand(sender As Object, e As TreeViewCancelEventArgs) Handles TreeViewArchivos.BeforeExpand
        'este evento se dispara antes de que un nodo se expanda, por lo que podemos cargar los subdirectorios en este momento
        Dim expandingNode As TreeNode = e.Node

        If expandingNode.Nodes.Count = 1 AndAlso expandingNode.Nodes(0).Text = "Loading..." Then
            expandingNode.Nodes.Clear()
            CargarSubDirectorios(expandingNode)
        End If
    End Sub

    Private Sub CargarSubDirectorios(parentNode As TreeNode)
        Dim dirInfo As DirectoryInfo = CType(parentNode.Tag, DirectoryInfo)

        Try
            ' Cargar subdirectorios
            For Each subDir As DirectoryInfo In dirInfo.GetDirectories()
                Dim dirNode As New TreeNode(subDir.Name)
                dirNode.Tag = subDir
                dirNode.Nodes.Add("Loading...") ' Añade un nodo temporal para permitir la expansión 
                parentNode.Nodes.Add(dirNode)
            Next
            ' Cargar archivos
            For Each file As FileInfo In dirInfo.GetFiles()
                Dim fileNode As New TreeNode(file.Name)
                fileNode.Tag = file
                parentNode.Nodes.Add(fileNode)
            Next
        Catch ex As UnauthorizedAccessException
            MessageBox.Show("Acceso denegado a " & dirInfo.FullName)
        End Try
    End Sub



    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        TreeViewArchivos.Nodes.Clear()

        For Each drive As DriveInfo In DriveInfo.GetDrives()
            If drive.IsReady Then
                Dim rootNode As New TreeNode(drive.Name)
                rootNode.Tag = drive.RootDirectory
                TreeViewArchivos.Nodes.Add(rootNode)
                CargarSubDirectorios(rootNode)
            End If
        Next
    End Sub

    Private Sub TreeViewArchivos_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles TreeViewArchivos.AfterSelect

    End Sub

    Private Sub TreeViewArchivos_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles TreeViewArchivos.NodeMouseClick
        If e.Button = MouseButtons.Right Then
            TreeViewArchivos.SelectedNode = e.Node
        End If
    End Sub

    Private Sub CopiarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CopiarToolStripMenuItem.Click
        Dim selectedNode As TreeNode = TreeViewArchivos.SelectedNode
        If selectedNode IsNot Nothing AndAlso TypeOf selectedNode.Tag Is FileInfo Then
            Dim fileInfo As FileInfo = CType(selectedNode.Tag, FileInfo)
            Dim saveFileDialog As New SaveFileDialog()
            saveFileDialog.FileName = fileInfo.Name
            If saveFileDialog.ShowDialog() = DialogResult.OK Then
                fileInfo.CopyTo(saveFileDialog.FileName, True)
                MessageBox.Show("Archivo copiado a " & saveFileDialog.FileName)
            End If
        ElseIf selectedNode IsNot Nothing AndAlso TypeOf selectedNode.Tag Is DirectoryInfo Then
            Dim dirInfo As DirectoryInfo = CType(selectedNode.Tag, DirectoryInfo)
            Dim folderBrowserDialog As New FolderBrowserDialog()
            If folderBrowserDialog.ShowDialog() = DialogResult.OK Then
                Dim destino As String = Path.Combine(folderBrowserDialog.SelectedPath, dirInfo.Name)
                DirectoryCopy(dirInfo.FullName, destino, True)
                MessageBox.Show("Directorio copiado a " & destino)
            End If
        End If
    End Sub

    ' Método auxiliar para copiar directorios
    Private Sub DirectoryCopy(sourceDir As String, destDir As String, copySubDirs As Boolean)
        Dim dir As DirectoryInfo = New DirectoryInfo(sourceDir)
        Dim dirs As DirectoryInfo() = dir.GetDirectories()

        If Not Directory.Exists(destDir) Then
            Directory.CreateDirectory(destDir)
        End If

        Dim files As FileInfo() = dir.GetFiles()
        For Each file As FileInfo In files
            Dim tempPath As String = Path.Combine(destDir, file.Name)
            file.CopyTo(tempPath, False)
        Next

        If copySubDirs Then
            For Each subDir As DirectoryInfo In dirs
                Dim tempPath As String = Path.Combine(destDir, subDir.Name)
                DirectoryCopy(subDir.FullName, tempPath, copySubDirs)
            Next
        End If
    End Sub

    Private Sub PegarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PegarToolStripMenuItem.Click
        Dim selectedNode As TreeNode = TreeViewArchivos.SelectedNode
        If selectedNode IsNot Nothing AndAlso TypeOf selectedNode.Tag Is FileInfo Then
            Dim fileInfo As FileInfo = CType(selectedNode.Tag, FileInfo)
            Dim folderBrowserDialog As New FolderBrowserDialog()
            If folderBrowserDialog.ShowDialog() = DialogResult.OK Then
                Dim destino As String = Path.Combine(folderBrowserDialog.SelectedPath, fileInfo.Name)
                fileInfo.MoveTo(destino)
                MessageBox.Show("Archivo movido a " & destino)
                selectedNode.Remove()
            End If
        ElseIf selectedNode IsNot Nothing AndAlso TypeOf selectedNode.Tag Is DirectoryInfo Then
            Dim dirInfo As DirectoryInfo = CType(selectedNode.Tag, DirectoryInfo)
            Dim folderBrowserDialog As New FolderBrowserDialog()
            If folderBrowserDialog.ShowDialog() = DialogResult.OK Then
                Dim destino As String = Path.Combine(folderBrowserDialog.SelectedPath, dirInfo.Name)
                dirInfo.MoveTo(destino)
                MessageBox.Show("Directorio movido a " & destino)
                selectedNode.Remove()
            End If
        End If
    End Sub

    Private Sub EliminarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EliminarToolStripMenuItem.Click
        Dim selectedNode As TreeNode = TreeViewArchivos.SelectedNode
        If selectedNode IsNot Nothing AndAlso TypeOf selectedNode.Tag Is FileInfo Then
            Dim fileInfo As FileInfo = CType(selectedNode.Tag, FileInfo)
            If MessageBox.Show("¿Está seguro de eliminar este archivo?", "Confirmación", MessageBoxButtons.YesNo) = DialogResult.Yes Then
                fileInfo.Delete()
                MessageBox.Show("Archivo eliminado")
                selectedNode.Remove()
            End If
        ElseIf selectedNode IsNot Nothing AndAlso TypeOf selectedNode.Tag Is DirectoryInfo Then
            Dim dirInfo As DirectoryInfo = CType(selectedNode.Tag, DirectoryInfo)
            If MessageBox.Show("¿Está seguro de eliminar este directorio?", "Confirmación", MessageBoxButtons.YesNo) = DialogResult.Yes Then
                dirInfo.Delete(True)
                MessageBox.Show("Directorio eliminado")
                selectedNode.Remove()
            End If
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Close()
    End Sub
End Class
