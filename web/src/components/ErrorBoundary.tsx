import { Component } from 'react'
import type { ReactNode, ErrorInfo } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'

interface Props {
  children: ReactNode
}

interface State {
  hasError: boolean
  error: Error | null
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props)
    this.state = { hasError: false, error: null }
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error }
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('ErrorBoundary caught:', error, info)
  }

  handleReset = () => {
    this.setState({ hasError: false, error: null })
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="flex items-center justify-center min-h-[400px] p-6">
          <Card className="max-w-md w-full">
            <CardHeader>
              <CardTitle className="text-destructive">შეცდომა მოხდა</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <p className="text-sm text-muted-foreground">
                გვერდის ჩატვირთვისას შეცდომა დაფიქსირდა. გთხოვთ სცადოთ თავიდან.
              </p>
              {this.state.error && (
                <p className="text-xs font-mono text-muted-foreground bg-muted p-2 rounded">
                  {this.state.error.message}
                </p>
              )}
              <Button onClick={this.handleReset} variant="outline" size="sm">
                თავიდან ცდა
              </Button>
            </CardContent>
          </Card>
        </div>
      )
    }

    return this.props.children
  }
}
