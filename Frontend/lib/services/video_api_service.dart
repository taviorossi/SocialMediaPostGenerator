import 'package:dio/dio.dart';

import '../config/api_config.dart';
import '../models/generate_video_response.dart';
import '../models/video_job_status_response.dart';

class VideoApiService {
  VideoApiService({String? baseUrl})
      : _dio = Dio(BaseOptions(
          baseUrl: baseUrl ?? kBackendBaseUrl,
          connectTimeout: const Duration(seconds: 30),
          receiveTimeout: const Duration(seconds: 30),
        ));

  final Dio _dio;

  Future<GenerateVideoResponse> uploadImage(String filePath, {String? theme}) async {
    final formData = FormData.fromMap({
      'image': await MultipartFile.fromFile(filePath),
      if (theme != null && theme.isNotEmpty) 'theme': theme,
    });
    final response = await _dio.post<Map<String, dynamic>>(
      '/api/video/generate',
      data: formData,
      options: Options(
        contentType: 'multipart/form-data',
        responseType: ResponseType.json,
      ),
    );
    final data = response.data;
    if (data == null) throw Exception('Resposta vazia');
    return GenerateVideoResponse.fromJson(Map<String, dynamic>.from(data));
  }

  Future<VideoJobStatusResponse> getJobStatus(String jobId) async {
    final response = await _dio.get<Map<String, dynamic>>(
      '/api/video/$jobId/status',
      options: Options(responseType: ResponseType.json),
    );
    final data = response.data;
    if (data == null) throw Exception('Resposta vazia');
    return VideoJobStatusResponse.fromJson(Map<String, dynamic>.from(data));
  }
}
