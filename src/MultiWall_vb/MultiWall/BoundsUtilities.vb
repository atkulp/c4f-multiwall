Public Class BoundsUtilities
    Public Shared Function AddBounds(ByVal sourceBounds As Rectangle, ByVal newBounds As Rectangle) As Rectangle
        If newBounds.Right > sourceBounds.Right Then
            sourceBounds.Width += (newBounds.Right - sourceBounds.Width)
        End If

        If newBounds.Bottom > sourceBounds.Bottom Then
            sourceBounds.Height += (newBounds.Bottom - sourceBounds.Height)
        End If

        If newBounds.Left < sourceBounds.Left Then
            sourceBounds.X = newBounds.X
        End If

        If newBounds.Top < sourceBounds.Top Then
            sourceBounds.Y = newBounds.Y
        End If

        Return sourceBounds
    End Function
End Class
