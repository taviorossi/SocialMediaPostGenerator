class VideoJobStatusResponse {
  final String jobId;
  final String status;
  final String? videoUrl;
  final String? errorMessage;
  final bool scriptGenerated;
  final bool audioGenerated;
  final bool renderSubmitted;

  VideoJobStatusResponse({
    required this.jobId,
    required this.status,
    this.videoUrl,
    this.errorMessage,
    this.scriptGenerated = false,
    this.audioGenerated = false,
    this.renderSubmitted = false,
  });

  factory VideoJobStatusResponse.fromJson(Map<String, dynamic> json) {
    final statusRaw = json['status'];
    final statusStr = statusRaw is int
        ? _statusFromInt(statusRaw)
        : (statusRaw as String? ?? 'Pending');
    return VideoJobStatusResponse(
      jobId: json['jobId']?.toString() ?? '',
      status: statusStr,
      videoUrl: json['videoUrl'] as String?,
      errorMessage: json['errorMessage'] as String?,
      scriptGenerated: json['scriptGenerated'] as bool? ?? false,
      audioGenerated: json['audioGenerated'] as bool? ?? false,
      renderSubmitted: json['renderSubmitted'] as bool? ?? false,
    );
  }

  static String _statusFromInt(int v) {
    switch (v) {
      case 0: return 'Pending';
      case 1: return 'ScriptGenerated';
      case 2: return 'AudioGenerated';
      case 3: return 'RenderSubmitted';
      case 4: return 'Done';
      case 5: return 'Failed';
      default: return 'Pending';
    }
  }

  String get statusLabel {
    switch (status) {
      case 'Pending':
        return 'Aguardando upload';
      case 'ScriptGenerated':
        return 'Gerando roteiro';
      case 'AudioGenerated':
        return 'Gerando áudio';
      case 'RenderSubmitted':
        return 'Renderizando';
      case 'Done':
        return 'Concluído';
      case 'Failed':
        return 'Erro';
      default:
        return status;
    }
  }
}
