import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search, Filter, BookOpen } from 'lucide-react';
import { SkeletonCard } from '../../components/ui';
import { NoResultsFound } from '../../components/ui/EmptyState';
import PageContainer from '../../components/layout/PageContainer';
import Badge from '../../components/ui/Badge';
import { getAllExercises, type Exercise, type CPAStage, type MathType } from '../../api/services/exerciseService';

const CPA_STAGE_LABELS: Record<CPAStage, { label: string; color: string }> = {
  concrete: { label: 'Concrete (Cụ thể)', color: 'bg-blue-100 text-blue-800 border-blue-200' },
  pictorial: { label: 'Pictorial (Hình ảnh)', color: 'bg-yellow-100 text-yellow-800 border-yellow-200' },
  abstract: { label: 'Abstract (Trừu tượng)', color: 'bg-purple-100 text-purple-800 border-purple-200' },
};

const MATH_TYPE_LABELS: Record<MathType, string> = {
  counting: 'Đếm số',
  comparison: 'So sánh',
  addition: 'Phép cộng',
  subtraction: 'Phép trừ',
};

const DifficultyStars = ({ level }: { level: 1 | 2 | 3 }) => (
  <div className="flex gap-0.5">
    {[1, 2, 3].map((star) => (
      <span
        key={star}
        className={`text-sm ${star <= level ? 'text-yellow-500' : 'text-gray-300'}`}
      >
        ★
      </span>
    ))}
  </div>
);

const ExerciseLibraryPage = () => {
  const navigate = useNavigate();
  const [exercises, setExercises] = useState<Exercise[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedStage, setSelectedStage] = useState<CPAStage | 'all'>('all');
  const [selectedMathType, setSelectedMathType] = useState<MathType | 'all'>('all');

  useEffect(() => {
    loadExercises();
  }, []);

  const loadExercises = async () => {
    try {
      setLoading(true);
      const data = await getAllExercises();
      setExercises(data);
    } catch (error) {
      console.error('Failed to load exercises:', error);
    } finally {
      setLoading(false);
    }
  };

  const filteredExercises = useMemo(() => exercises.filter((exercise) => {
    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      if (
        !exercise.title.toLowerCase().includes(query) &&
        !exercise.description.toLowerCase().includes(query)
      ) {
        return false;
      }
    }

    if (selectedStage !== 'all' && exercise.cpaStage !== selectedStage) {
      return false;
    }

    if (selectedMathType !== 'all' && exercise.mathType !== selectedMathType) {
      return false;
    }

    return true;
  }), [exercises, searchQuery, selectedStage, selectedMathType]);

  if (loading) {
    return (
      <div className="min-h-screen bg-light-bg px-4 sm:px-6 lg:px-8 py-6 space-y-6">
        <div className="rounded-2xl border border-gray-100 bg-white p-6 shadow-soft animate-pulse">
          <div className="h-8 w-64 rounded-lg bg-gray-200 skeleton mb-2" />
          <div className="h-4 w-80 max-w-full rounded-lg bg-gray-200 skeleton" />
        </div>

        <div className="rounded-2xl border border-gray-100 bg-white p-4 shadow-soft space-y-4">
          <div className="grid grid-cols-1 gap-4 lg:grid-cols-[minmax(0,1fr)_auto]">
            <div className="h-12 rounded-xl bg-gray-200 skeleton" />
            <div className="flex flex-wrap gap-2">
              <div className="h-10 w-20 rounded-xl bg-gray-200 skeleton" />
              <div className="h-10 w-20 rounded-xl bg-gray-200 skeleton" />
              <div className="h-10 w-20 rounded-xl bg-gray-200 skeleton" />
            </div>
          </div>
          <div className="flex flex-wrap gap-2">
            <div className="h-8 w-20 rounded-full bg-gray-200 skeleton" />
            <div className="h-8 w-24 rounded-full bg-gray-200 skeleton" />
            <div className="h-8 w-28 rounded-full bg-gray-200 skeleton" />
          </div>
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {Array.from({ length: 6 }).map((_, index) => (
            <SkeletonCard key={index} />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-light-bg">
      <PageContainer maxWidth="xl" padding="md" spacing="md" className="py-4 sm:py-6">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Thư viện bài tập</h1>
        <p className="text-gray-600">
          Hơn {exercises.length} bài tập CPA được thiết kế riêng cho trẻ Dyscalculia
        </p>
      </div>

      {/* Search and Filters */}
      <div className="bg-white rounded-2xl shadow-soft border border-gray-100 p-4 mb-6">
        {/* Search */}
        <div className="relative w-full">
          <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          <input
            type="text"
            placeholder="Tìm kiếm bài tập..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full pl-11 pr-5 py-3 border border-gray-200 rounded-full focus:outline-none focus:ring-2 focus:ring-primary-500"
          />
        </div>

        {/* Filters Row */}
        <div className="flex flex-wrap items-center gap-y-3 gap-x-4 mt-4 overflow-x-auto pb-1">
          {/* Math Type Filter */}
          <div className="flex items-center flex-wrap gap-2">
            <span className="text-sm text-gray-600 flex items-center gap-1.5 font-medium">
              <Filter className="w-4 h-4" />
              Loại toán:
            </span>
            <button
              onClick={() => setSelectedMathType('all')}
              className={`px-3 py-1 rounded-full text-sm border transition-colors ${
                selectedMathType === 'all'
                  ? 'bg-[#64aaeb] text-white border-[#64aaeb]'
                  : 'bg-white text-gray-700 border-[#64aaeb]/50 hover:bg-[#64aaeb]/5 hover:border-[#64aaeb]'
              }`}
            >
              Tất cả
            </button>
            {(Object.keys(MATH_TYPE_LABELS) as MathType[]).map((type) => (
              <button
                key={type}
                onClick={() => setSelectedMathType(type)}
                className={`px-3 py-1 rounded-full text-sm border transition-colors ${
                  selectedMathType === type
                    ? 'bg-[#64aaeb] text-white border-[#64aaeb]'
                    : 'bg-white text-gray-700 border-[#64aaeb]/50 hover:bg-[#64aaeb]/5 hover:border-[#64aaeb]'
                }`}
              >
                {MATH_TYPE_LABELS[type]}
              </button>
            ))}
          </div>

          {/* Separator */}
          <span className="text-gray-300 mx-1 hidden sm:inline">|</span>

          {/* CPA Stage Filter */}
          <div className="flex items-center flex-wrap gap-2">
            <span className="text-sm text-gray-600 flex items-center gap-1.5 font-medium">
              <BookOpen className="w-4 h-4" />
              Phương pháp:
            </span>
            {[
              { id: 'all' as const, label: 'Tất cả' },
              { id: 'concrete' as const, label: 'Concrete' },
              { id: 'pictorial' as const, label: 'Pictorial' },
              { id: 'abstract' as const, label: 'Abstract' }
            ].map((btn) => (
              <button
                key={btn.id}
                onClick={() => setSelectedStage(btn.id)}
                className={`px-3 py-1 rounded-full text-sm border font-medium transition-colors ${
                  selectedStage === btn.id
                    ? 'bg-[#64aaeb] text-white border-[#64aaeb]'
                    : 'bg-white text-gray-700 border-[#64aaeb]/50 hover:bg-[#64aaeb]/5 hover:border-[#64aaeb]'
                }`}
              >
                {btn.label}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Results count */}
      <p className="text-sm text-gray-500 mb-4">
        Hiển thị {filteredExercises.length} / {exercises.length} bài tập
      </p>

      {/* Exercise Grid */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
        {filteredExercises.map((exercise) => (
          <div
            key={exercise.id}
            className="bg-white rounded-2xl shadow-soft border border-gray-100 p-5 hover:shadow-medium transition-shadow cursor-pointer"
            onClick={() => navigate('/parent/exercises')}
          >
            {/* CPA Stage Badge */}
            <div className="flex items-start justify-between mb-3">
              <Badge className={CPA_STAGE_LABELS[exercise.cpaStage].color}>
                {CPA_STAGE_LABELS[exercise.cpaStage].label}
              </Badge>
              <DifficultyStars level={exercise.difficulty} />
            </div>

            {/* Title & Description */}
            <h3 className="font-bold text-gray-900 mb-2">{exercise.title}</h3>
            <p className="text-sm text-gray-600 mb-3 line-clamp-2">
              {exercise.description}
            </p>

            {/* Tags */}
            <div className="flex flex-wrap gap-1 mb-3">
              <Badge variant="default" className="text-xs">
                {MATH_TYPE_LABELS[exercise.mathType]}
              </Badge>
              <Badge variant="default" className="text-xs">
                {exercise.numberRange.min}-{exercise.numberRange.max}
              </Badge>
            </div>

            {/* Meta */}
            <div className="flex items-center gap-4 text-sm text-gray-500">
              <span className="flex items-center gap-1">
                <BookOpen className="w-4 h-4" />
                {exercise.duration} phút
              </span>
              <span>
                {exercise.prerequisites.length > 0
                  ? `Cần ${exercise.prerequisites.length} bài trước`
                  : 'Có thể bắt đầu'}
              </span>
            </div>
          </div>
        ))}
      </div>

      {/* Empty State */}
      {filteredExercises.length === 0 && (
        <div className="rounded-2xl border border-dashed border-gray-200 bg-white">
          <NoResultsFound onReset={() => {
            setSearchQuery('');
            setSelectedStage('all');
            setSelectedMathType('all');
          }} />
        </div>
      )}
      </PageContainer>
    </div>
  );
};

export default ExerciseLibraryPage;
