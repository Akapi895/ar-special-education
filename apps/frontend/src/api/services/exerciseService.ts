/**
 * Exercise Service
 * Mock service for CPA exercises (no backend)
 */

import { mockExercises } from '../mockData/exercises';

export type CPAStage = 'concrete' | 'pictorial' | 'abstract';
export type MathType = 'counting' | 'comparison' | 'addition' | 'subtraction';
export type Difficulty = 1 | 2 | 3;

export interface Exercise {
  id: string;
  title: string;
  description: string;
  duration: number; // minutes
  
  cpaStage: CPAStage;
  mathType: MathType;
  numberRange: { min: number; max: number };
  difficulty: Difficulty;
  
  concrete: {
    instructions: string;
    materials: string[];
    examples: string[];
  };
  pictorial: {
    instructions: string;
    visualTypes: string[];
  };
  abstract: {
    instructions: string;
    examples: string[];
    warnings: string[];
  };
  
  parentTips: string[];
  commonMistakes: string[];
  warningSigns: string[];
  
  prerequisites: string[];
  nextExercises: string[];
  
  tags: string[];
}

export interface ExerciseFilter {
  cpaStage?: CPAStage;
  mathType?: MathType;
  difficulty?: Difficulty;
  tags?: string[];
}

// Simulate network delay
const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

export const getAllExercises = async (): Promise<Exercise[]> => {
  await delay(100);
  return mockExercises;
};

export const getExerciseById = async (id: string): Promise<Exercise | undefined> => {
  await delay(50);
  return mockExercises.find(e => e.id === id);
};

export const getExercisesByFilter = async (filter: ExerciseFilter): Promise<Exercise[]> => {
  await delay(100);
  return mockExercises.filter(exercise => {
    if (filter.cpaStage && exercise.cpaStage !== filter.cpaStage) return false;
    if (filter.mathType && exercise.mathType !== filter.mathType) return false;
    if (filter.difficulty && exercise.difficulty !== filter.difficulty) return false;
    if (filter.tags && filter.tags.length > 0) {
      const hasAllTags = filter.tags.every(tag => exercise.tags.includes(tag));
      if (!hasAllTags) return false;
    }
    return true;
  });
};

export const getSuggestedExercises = async (
  _childId: string,
  currentProgress: { concrete: number; pictorial: number; abstract: number }
): Promise<Exercise[]> => {
  await delay(100);
  
  // Suggest exercises based on child's current CPA progress
  let targetStage: CPAStage = 'concrete';
  if (currentProgress.concrete < 50) {
    targetStage = 'concrete';
  } else if (currentProgress.pictorial < 50) {
    targetStage = 'pictorial';
  } else {
    targetStage = 'abstract';
  }
  
  // Filter exercises at the target stage with difficulty 1-2
  const suggestions = mockExercises
    .filter(e => e.cpaStage === targetStage && e.difficulty <= 2)
    .slice(0, 5);
  
  return suggestions;
};

export const getExercisesByCPAStage = async (stage: CPAStage): Promise<Exercise[]> => {
  await delay(100);
  return mockExercises.filter(e => e.cpaStage === stage);
};

export const getExercisesByMathType = async (mathType: MathType): Promise<Exercise[]> => {
  await delay(100);
  return mockExercises.filter(e => e.mathType === mathType);
};

export default {
  getAllExercises,
  getExerciseById,
  getExercisesByFilter,
  getSuggestedExercises,
  getExercisesByCPAStage,
  getExercisesByMathType,
};
