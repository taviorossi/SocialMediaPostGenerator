class GenerateVideoResponse {
  final String jobId;
  final String status;
  final String message;

  GenerateVideoResponse({
    required this.jobId,
    required this.status,
    required this.message,
  });

  factory GenerateVideoResponse.fromJson(Map<String, dynamic> json) {
    final statusRaw = json['status'];
    final statusStr = statusRaw is int
        ? _statusFromInt(statusRaw)
        : (statusRaw as String? ?? 'Pending');
    return GenerateVideoResponse(
      jobId: json['jobId']?.toString() ?? '',
      status: statusStr,
      message: json['message'] as String? ?? '',
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
}
